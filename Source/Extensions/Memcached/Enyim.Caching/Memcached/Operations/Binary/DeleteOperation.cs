using PHP.Library.Memcached;

namespace Enyim.Caching.Memcached.Operations.Binary
{
    internal class DeleteOperation : ItemOperation
    {
        private readonly int delay;

        public DeleteOperation(IServerPool pool, string key, string serverKey, int delay)
            : base(pool, key, serverKey, BinaryProtocol.MaxKeyLength)
        {
            this.delay = delay;
        }

        protected override ResConstants ExecuteAction()
        {
            if (delay > 0)
                return ResConstants.ProtocolError;

            PooledSocket socket = this.Socket;
            if (socket == null)
                return ResConstants.ConnectionSocketCreateFailure;

            BinaryRequest request = new BinaryRequest(OpCode.Delete);
            request.Key = this.HashedKey;
            request.Write(socket);

            BinaryResponse response = new BinaryResponse();

            return response.Read(socket);
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
