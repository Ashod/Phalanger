using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Enyim.Caching.Memcached.Distribution
{
	/// <summary>
	/// Implements Ketama cosistent hashing, compatible with the "spymemcached" Java client
	/// </summary>
    public sealed class KetamaNodeLocator : IMemcachedNodeLocator
	{
		private const int ServerAddressMutations = 160;

		private List<IMemcachedNode> servers;

        private HashAlgorithm hash;

		// holds all server keys for mapping an item key to the server consistently
		private List<uint> keys;
		// used to lookup a server based on its key
		private Dictionary<uint, IMemcachedNode> keyToServer;

		private bool isInitialized;

        void IMemcachedNodeLocator.Initialize(ICollection<IMemcachedNode> nodes, HashAlgorithm hash)
		{
			if (this.isInitialized) throw new InvalidOperationException("Instance is already initialized.");

            this.hash = hash;

			// sizeof(uint)
			const int KeyLength = 4;
            
			int PartCount = hash.HashSize / 8 / KeyLength; // HashSize is in bits, uint is 4 bytes long
			if (PartCount < 1) throw new ArgumentOutOfRangeException("The hash algorithm must provide at least 32 bits long hashes");

			var keys = new List<uint>(nodes.Count * ServerAddressMutations);
			var keyToServer = new Dictionary<uint, IMemcachedNode>(keys.Count, new UIntEqualityComparer());

            foreach (var currentNode in nodes)
			//for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
			{
				//var currentNode = nodes[nodeIndex];

				// every server is registered numberOfKeys times
				// using UInt32s generated from the different parts of the hash
				// i.e. hash is 64 bit:
				// 01 02 03 04 05 06 07
				// server will be stored with keys 0x07060504 & 0x03020100
				string address = currentNode.EndPoint.ToString();

				for (int mutation = 0; mutation < ServerAddressMutations / PartCount; mutation++)
				{
                    byte[] data = hash.ComputeHash(Encoding.ASCII.GetBytes(address + "-" + mutation));

					for (int p = 0; p < PartCount; p++)
					{
						var tmp = p * 4;
						var key = ((uint)data[tmp + 3] << 24)
									| ((uint)data[tmp + 2] << 16)
									| ((uint)data[tmp + 1] << 8)
									| ((uint)data[tmp]);

						keys.Add(key);
						keyToServer[key] = currentNode;
					}
				}
			}

			keys.Sort();

			this.keys = keys;
			this.keyToServer = keyToServer;

			this.servers = new List<IMemcachedNode>();
			this.servers.AddRange(nodes);

			this.isInitialized = true;
		}

		private uint GetKeyHash(string key)
		{
            return BitConverter.ToUInt32(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0);
		}

		IMemcachedNode IMemcachedNodeLocator.Locate(string key)
		{
			if (!this.isInitialized) throw new InvalidOperationException("You must call Initialize first");
			if (key == null) throw new ArgumentNullException("key");
			if (this.servers.Count == 0) return null;
			if (this.servers.Count == 1) return this.servers[0];

			var retval = this.LocateNode(this.GetKeyHash(key));

			// isalive is not atomic
			if (!retval.IsAlive)
			{
				for (var i = 0; i < this.servers.Count; i++)
				{
					// -- this is from spymemcached so we select the same node for the same items
					ulong tmpKey = (ulong)GetKeyHash(i + key);
					tmpKey += (uint)(tmpKey ^ (tmpKey >> 32));
					tmpKey &= 0xffffffffL; /* truncate to 32-bits */
					// -- end

					retval = this.LocateNode((uint)tmpKey);

					if (retval.IsAlive) return retval;
				}
			}

			return retval.IsAlive ? retval : null;
		}

		private IMemcachedNode LocateNode(uint itemKeyHash)
		{
			// get the index of the server assigned to this hash
			int foundIndex = this.keys.BinarySearch(itemKeyHash);

			// no exact match
			if (foundIndex < 0)
			{
				// this is the nearest server in the list
				foundIndex = ~foundIndex;

				if (foundIndex == 0)
				{
					// it's smaller than everything, so use the last server (with the highest key)
					foundIndex = this.keys.Count - 1;
				}
				else if (foundIndex >= this.keys.Count)
				{
					// the key was larger than all server keys, so return the first server
					foundIndex = 0;
				}
			}

			if (foundIndex < 0 || foundIndex > this.keys.Count)
				return null;

			return this.keyToServer[this.keys[foundIndex]];
		}
	}
}

#region [ License information          ]
/* ************************************************************
 * 
 *    Copyright (c) 2010 Attila Kisk�, enyim.com
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion
