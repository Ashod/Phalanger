using System;
using System.Collections.Generic;
using PHP.Library.Memcached;

namespace Enyim.Caching.Memcached.Operations.Binary
{
	internal class FlushOperation : Operation
	{
        private readonly int delay;

        public FlushOperation(IServerPool pool, int delay) : base(pool)
        {
            this.delay = delay;
        }

		protected override ResConstants ExecuteAction()
		{
			IList<ArraySegment<byte>> request = null;

			foreach (IMemcachedNode server in this.ServerPool.GetServers())
			{
				if (!server.IsAlive) continue;

				if (request == null)
				{
					BinaryRequest bq = new BinaryRequest(OpCode.FlushQ);
                    
                    if (delay > 0)
                    {
                        byte[] extra = new byte[4];
                        BinaryConverter.EncodeUInt32((uint)delay, extra, 0);
                        bq.Extra = new ArraySegment<byte>(extra);
                    }

					request = bq.CreateBuffer();
				}

				using (PooledSocket socket = server.Acquire())
				{
					if (socket != null)
                        socket.Write(request);
				}
			}

			return ResConstants.Success;
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
