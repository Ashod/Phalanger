﻿using System;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System.Collections.Generic;
using PHP.Library.Memcached;

namespace Enyim.Caching
{
    /// <summary>
    /// Memcached client.
    /// </summary>
    public class MemcachedClient : IDisposable
    {
        /// <summary>
        /// Represents a value which indicates that an item should never expire.
        /// </summary>
        public static readonly TimeSpan Infinite = TimeSpan.Zero;

        //internal static MemcachedClientSection DefaultSettings = ConfigurationManager.GetSection("enyim.com/memcached") as MemcachedClientSection;

        private IProtocolImplementation protImpl;

        /// <summary>
        /// Initializes a new MemcachedClient instance using the default configuration section (enyim/memcached).
        /// </summary>
        //public MemcachedClient()
        //{
        //    this.Initialize(DefaultSettings);
        //}

        ~MemcachedClient()
        {
            try { ((IDisposable)this).Dispose(); }
            catch { }
        }

        ///// <summary>
        ///// Initializes a new MemcachedClient instance using the specified configuration section. 
        ///// This overload allows to create multiple MemcachedClients with different pool configurations.
        ///// </summary>
        ///// <param name="sectionName">The name of the configuration section to be used for configuring the behavior of the client.</param>
        //public MemcachedClient(string sectionName)
        //{
        //    MemcachedClientSection section = (MemcachedClientSection)ConfigurationManager.GetSection(sectionName);
        //    if (section == null)
        //        throw new ConfigurationErrorsException("Section " + sectionName + " is not found.");

        //    this.Initialize(section);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MemcachedClient"/> using a custom server pool implementation.
        /// </summary>
        /// <param name="pool">The server pool this client should use</param>
        /// <param name="provider">The authentication provider this client should use. If null, the connections will not be authenticated.</param>
        /// <param name="protocol">Specifies which protocol the client should use to communicate with the servers.</param>
        public MemcachedClient(IServerPool pool, ISaslAuthenticationProvider provider, MemcachedProtocol protocol)
        {
            if (pool == null)
                throw new ArgumentNullException("pool");

            this.Initialize(pool, provider, protocol);
        }

        private static ISaslAuthenticationProvider GetProvider(IMemcachedClientConfiguration configuration)
        {
            // create&initialize the authenticator, if any
            // we'll use this single instance everywhere, so it must be thread safe
            IAuthenticationConfiguration auth = configuration.Authentication;
            if (auth != null)
            {
                Type t = auth.Type;
                var provider = (t == null) ? null : Enyim.Reflection.FastActivator.CreateInstance(t) as ISaslAuthenticationProvider;

                if (provider != null)
                {
                    provider.Initialize(auth.Parameters);
                    return provider;
                }
            }

            return null;
        }

        private void Initialize(IServerPool pool, ISaslAuthenticationProvider provider, MemcachedProtocol protocol)
        {
            IProtocolImplementation protImpl;

            switch (protocol)
            {
                case MemcachedProtocol.Binary:
                    protImpl = new Enyim.Caching.Memcached.Operations.Binary.BinaryProtocol(pool);
                    break;

                case MemcachedProtocol.Text:
                    protImpl = new Enyim.Caching.Memcached.Operations.Text.TextProtocol(pool);
                    break;

                default: throw new ArgumentOutOfRangeException("Unknown protocol: " + protocol);
            }

            if (provider != null)
                pool.Authenticator = protImpl.CreateAuthenticator(provider);

            this.protImpl = protImpl;

            // everything is initialized, start the pool
            pool.Start();
        }

        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to retrieve.</param>
        /// <param name="cas"></param>
        /// <returns>The retrieved item, or <value>null</value> if the key was not found.</returns>
        public object Get(string serverKey, string key, out ulong cas)
        {
            ResultObj result;
            this.protImpl.TryGet(serverKey, key, out result);

            cas = result.cas;
            return result.value;
        }

        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to retrieve.</param>
        /// <param name="cas"></param>
        /// <returns>The retrieved item, or <value>default(T)</value> if the key was not found.</returns>
        public T Get<T>(string serverKey, string key, out ulong cas)
        {
            object tmp;

            cas = 0;

            return TryGet(serverKey, key, out tmp, out cas) ? (T)tmp : default(T);
        }
        /// <summary>
        /// Tries to get an item from the cache.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to retrieve.</param>
        /// <param name="value">The retrieved item or null if not found.</param>
        /// <param name="cas"></param>
        /// <returns>The <value>true</value> if the item was successfully retrieved.</returns>
        public bool TryGet(string serverKey, string key, out object value, out ulong cas)
        {
            ResultObj result;
            if (this.protImpl.TryGet(serverKey, key, out result) == ResConstants.Success)
            {
                value = result.value;
                cas = result.cas;
                return true;
            }
            else
            {
                value = null;
                cas = 0;
                return false;
            }
        }

        /// <summary>
        /// Inserts an item into the cache with a cache key to reference its location.
        /// </summary>
        /// <param name="mode">Defines how the item is stored in the cache.</param>
        /// <param name="serverKey"></param>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="cas"></param>
        /// <remarks>The item does not expire unless it is removed due memory pressure.</remarks>
        /// <returns>true if the item was successfully stored in the cache; false otherwise.</returns>
        public bool Store(StoreMode mode, string serverKey, string key, object value, ulong cas)
        {
            return this.protImpl.Store(mode, serverKey, key, value, cas, 0) == ResConstants.Success;
        }

        /// <summary>
        /// Inserts an item into the cache with a cache key to reference its location.
        /// </summary>
        /// <param name="mode">Defines how the item is stored in the cache.</param>
        /// <param name="serverKey"></param>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="cas"></param>
        /// <param name="validFor">The interval after the item is invalidated in the cache.</param>
        /// <returns>true if the item was successfully stored in the cache; false otherwise.</returns>
        public bool Store(StoreMode mode, string serverKey, string key, object value, ulong cas, TimeSpan validFor)
        {
            return this.protImpl.Store(mode, serverKey, key, value, cas, MemcachedClient.GetExpiration(validFor, null)) == ResConstants.Success;
        }

        /// <summary>
        /// Inserts an item into the cache with a cache key to reference its location.
        /// </summary>
        /// <param name="mode">Defines how the item is stored in the cache.</param>
        /// <param name="serverKey"></param>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="cas"></param>
        /// <param name="expiresAt">The time when the item is invalidated in the cache.</param>
        /// <returns>true if the item was successfully stored in the cache; false otherwise.</returns>
        public bool Store(StoreMode mode, string serverKey, string key, object value, ulong cas, DateTime expiresAt)
        {
            return this.protImpl.Store(mode, serverKey, key, value, cas, MemcachedClient.GetExpiration(null, expiresAt)) == ResConstants.Success;
        }

        /// <summary>
        /// Inserts an item into the cache with a cache key to reference its location.
        /// </summary>
        /// <param name="mode">Defines how the item is stored in the cache.</param>
        /// <param name="serverKey"></param>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="cas"></param>
        /// <param name="expiration">The time when the item is invalidated in the cache.</param>
        /// <returns>true if the item was successfully stored in the cache; false otherwise.</returns>
        public bool Store(StoreMode mode, string serverKey, string key, object value, ulong cas, uint expiration)
        {
            return this.protImpl.Store(mode, serverKey, key, value, cas, expiration) == ResConstants.Success;
        }

        /// <summary>
        /// Increments the value of the specified key by the given amount. The operation is atomic and happens on the server.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to increment.</param>
        /// <param name="defaultValue">The value which will be stored by the server if the specified item was not found.</param>
        /// <param name="delta">The amount by which the client wants to increase the item.</param>
        /// <returns>The new value of the item or defaultValue if the key was not found.</returns>
        /// <remarks>If the client uses the Text protocol, the item must be inserted into the cache before it can be changed. It must be inserted as a <see cref="T:System.String"/>. Moreover the Text protocol only works with <see cref="System.UInt32"/> values, so return value -1 always indicates that the item was not found.</remarks>
        public ulong Increment(string serverKey, string key, ulong defaultValue, ulong delta)
        {
            ulong newVal;
            this.protImpl.Mutate(MutationMode.Increment, serverKey, key, defaultValue, delta, false, out newVal);
            return newVal;
        }

        /// <summary>
        /// Decrements the value of the specified key by the given amount. The operation is atomic and happens on the server.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to decrement.</param>
        /// <param name="defaultValue">The value which will be stored by the server if the specified item was not found.</param>
        /// <param name="delta">The amount by which the client wants to decrease the item.</param>
        /// <returns>The new value of the item or defaultValue if the key was not found.</returns>
        /// <remarks>If the client uses the Text protocol, the item must be inserted into the cache before it can be changed. It must be inserted as a <see cref="T:System.String"/>. Moreover the Text protocol only works with <see cref="System.UInt32"/> values, so return value -1 always indicates that the item was not found.</remarks>
        public ulong Decrement(string serverKey, string key, ulong defaultValue, ulong delta)
        {
            ulong newVal;
            this.protImpl.Mutate(MutationMode.Decrement, serverKey, key, defaultValue, delta, false, out newVal);
            return newVal;
        }

        /// <summary>
        /// Appends the data to the end of the specified item's data on the server.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="data">The data to be stored.</param>
        /// <returns>true if the data was successfully stored; false otherwise.</returns>
        public bool Append(string serverKey, string key, ArraySegment<byte> data)
        {
            return this.protImpl.Concatenate(ConcatenationMode.Append, serverKey, key, data) == ResConstants.Success;
        }

        /// <summary>
        /// Inserts the data before the specified item's data on the server.
        /// </summary>
        /// <returns>true if the data was successfully stored; false otherwise.</returns>
        public bool Prepend(string serverKey, string key, ArraySegment<byte> data)
        {
            return this.protImpl.Concatenate(ConcatenationMode.Prepend, serverKey, key, data) == ResConstants.Success;
        }

        /// <summary>
        /// Removes all data from the cache. Note: this will invalidate all data on all servers in the pool.
        /// </summary>
        public void FlushAll()
        {
            this.protImpl.FlushAll(0);
        }

        /// <summary>
        /// Returns statistics about the servers.
        /// </summary>
        /// <returns></returns>
        public ServerStats Stats()
        {
            ServerStats stats;
            this.protImpl.Stats(out stats);
            return stats;
        }

        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="key">The identifier for the item to delete.</param>
        /// <returns>true if the item was successfully removed from the cache; false otherwise.</returns>
        public bool Remove(string serverKey, string key)
        {
            return this.protImpl.Remove(serverKey, key, 0) == ResConstants.Success;
        }

        /// <summary>
        /// Retrieves multiple items from the cache.
        /// </summary>
        /// <param name="serverKey"></param>
        /// <param name="keys">The list of identifiers for the items to retrieve.</param>
        /// <returns>a Dictionary holding all items indexed by their key.</returns>
        public IDictionary<string, ResultObj> Get(string serverKey, IEnumerable<string> keys)
        {
            IDictionary<string,ResultObj> result;
            this.protImpl.Get(serverKey, keys, out result);
            return result;
        }

        #region [ Expiration helper            ]
        private const int MaxSeconds = 60 * 60 * 24 * 30;
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validFor"></param>
        /// <param name="expiresAt"></param>
        /// <returns></returns>
        public static uint GetExpiration(TimeSpan? validFor, DateTime? expiresAt)
        {
            if (validFor != null && expiresAt != null)
                throw new ArgumentException("You cannot specify both validFor and expiresAt.");

            if (expiresAt != null)
            {
                DateTime dt = expiresAt.Value;

                if (dt < UnixEpoch)
                    throw new ArgumentOutOfRangeException("expiresAt", "expiresAt must be >= 1970/1/1");

                // accept MaxValue as infinite
                if (dt == DateTime.MaxValue)
                    return 0;

                uint retval = (uint)(dt.ToUniversalTime() - UnixEpoch).TotalSeconds;

                return retval;
            }

            TimeSpan ts = validFor.Value;

            // accept Zero as infinite
            if (ts.TotalSeconds >= MaxSeconds || ts < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("validFor", "validFor must be < 30 days && >= 0");

            return (uint)ts.TotalSeconds;
        }
        #endregion
        #region [ IDisposable                  ]
        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources allocated by this instance
        /// </summary>
        /// <remarks>Technically it's not really neccesary to call this, since the client does not create "really" disposable objects, so it's safe to assume that when 
        /// the AppPool shuts down all resources will be released correctly and no handles or such will remain in the memory.</remarks>
        public void Dispose()
        {
            if (this.protImpl != null)
            {
                GC.SuppressFinalize(this);

                try
                {
                    this.protImpl.Dispose();
                }
                finally
                {
                    this.protImpl = null;
                }
            }
        }
        #endregion
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
