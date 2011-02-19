using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enyim.Caching.Configuration
{
	public class SocketPoolConfiguration : ISocketPoolConfiguration
	{
		private int minPoolSize = 10;
		private int maxPoolSize = 200;
		private TimeSpan connectionTimeout = new TimeSpan(0, 0, 10);
		private TimeSpan receiveTimeout = new TimeSpan(0, 0, 10);
		private TimeSpan deadTimeout = new TimeSpan(0, 2, 0);

		int ISocketPoolConfiguration.MinPoolSize
		{
			get { return this.minPoolSize; }
			set
			{
				if (value > 1000 || value > this.maxPoolSize)
					throw new ArgumentOutOfRangeException("value", "MinPoolSize must be <= MaxPoolSize and must be <= 1000");

				this.minPoolSize = value;
			}
		}

		int ISocketPoolConfiguration.MaxPoolSize
		{
			get { return this.maxPoolSize; }
			set
			{
				if (value > 1000 || value < this.minPoolSize)
					throw new ArgumentOutOfRangeException("value", "MaxPoolSize must be >= MinPoolSize and must be <= 1000");

				this.maxPoolSize = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.ConnectionTimeout
		{
			get { return this.connectionTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				this.connectionTimeout = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.ReceiveTimeout
		{
			get { return this.receiveTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				this.receiveTimeout = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.DeadTimeout
		{
			get { return this.deadTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				this.deadTimeout = value;
			}
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
