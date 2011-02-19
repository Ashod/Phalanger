using System;
using System.Collections.Generic;
using System.Net;
using PHP.Library.Memcached;

namespace Enyim.Caching.Memcached.Operations.Text
{
	internal sealed class StatsOperation : Operation
	{
		//private log4net.ILog log = log4net.LogManager.GetLogger(typeof(StatsOperation));

		private ServerStats results;

		public StatsOperation(IServerPool pool) : base(pool) { }

		public ServerStats Results
		{
			get { return this.results; }
		}

		protected override ResConstants ExecuteAction()
		{
            Dictionary<NamedIPEndPoint, Dictionary<string, string>> retval = new Dictionary<NamedIPEndPoint, Dictionary<string, string>>();

			foreach (IMemcachedNode server in this.ServerPool.GetServers())
			{
				if (!server.IsAlive) continue;

				using (PooledSocket socket = server.Acquire())
				{
					if (socket == null)
						continue;

					try
					{
						TextSocketHelper.SendCommand(socket, "stats");
						if (!socket.IsAlive) continue;

						Dictionary<string, string> serverData = new Dictionary<string, string>(StringComparer.Ordinal);

						while (true)
						{
							string line = TextSocketHelper.ReadResponse(socket);

							// stat values are terminated by END
							if (String.Compare(line, "END", StringComparison.Ordinal) == 0)
								break;

							// expected response is STAT item_name item_value
							if (line.Length < 6 || String.Compare(line, 0, "STAT ", 0, 5, StringComparison.Ordinal) != 0)
							{
                                // TODO: warning
                                //if (log.IsWarnEnabled)
                                //    log.Warn("Unknow response: " + line);

								continue;
							}

							// get the key&value
							string[] parts = line.Remove(0, 5).Split(' ');
							if (parts.Length != 2)
							{
                                //if (log.IsWarnEnabled)
                                //    log.Warn("Unknow response: " + line);

								continue;
							}

							// store the stat item
							serverData[parts[0]] = parts[1];
						}

						retval[server.EndPoint] = serverData;
					}
					catch (Exception /*e*/)
					{
                        // TODO: throw PHP error
						//log.Error(e);
					}
				}
			}

			this.results = new ServerStats(retval);

            //
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
