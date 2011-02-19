using System;
using System.ComponentModel;
using System.Configuration;

namespace Enyim.Caching.Configuration
{
	/// <summary>
	/// Configures the socket pool settings for Memcached servers.
	/// </summary>
	public sealed class SocketPoolElement : ConfigurationElement, ISocketPoolConfiguration
	{
		/// <summary>
		/// Gets or sets a value indicating the minimum amount of sockets per server in the socket pool.
		/// </summary>
		/// <returns>The minimum amount of sockets per server in the socket pool.</returns>
		[ConfigurationProperty("minPoolSize", IsRequired = false, DefaultValue = 10), IntegerValidator(MinValue = 0, MaxValue = 1000)]
		public int MinPoolSize
		{
			get { return (int)base["minPoolSize"]; }
			set { base["minPoolSize"] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating the maximum amount of sockets per server in the socket pool.
		/// </summary>
		/// <returns>The maximum amount of sockets per server in the socket pool.</returns>
		[ConfigurationProperty("maxPoolSize", IsRequired = false, DefaultValue = 200), IntegerValidator(MinValue = 0, MaxValue = 1000)]
		public int MaxPoolSize
		{
			get { return (int)base["maxPoolSize"]; }
			set { base["maxPoolSize"] = value; }
		}

		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which the connection attempt will fail.
		/// </summary>
		/// <returns>The value of the connection timeout. The default is 10 seconds.</returns>
		[ConfigurationProperty("connectionTimeout", IsRequired = false, DefaultValue = "00:00:10"), PositiveTimeSpanValidator, TypeConverter(typeof(InfiniteTimeSpanConverter))]
		public TimeSpan ConnectionTimeout
		{
			get { return (TimeSpan)base["connectionTimeout"]; }
			set { base["connectionTimeout"] = value; }
		}

		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which receiving data from the socket fails.
		/// </summary>
		/// <returns>The value of the receive timeout. The default is 10 seconds.</returns>
		[ConfigurationProperty("receiveTimeout", IsRequired = false, DefaultValue = "00:00:10"), PositiveTimeSpanValidator, TypeConverter(typeof(InfiniteTimeSpanConverter))]
		public TimeSpan ReceiveTimeout
		{
			get { return (TimeSpan)base["receiveTimeout"]; }
			set { base["receiveTimeout"] = value; }
		}

		/// <summary>
		/// Gets or sets a value that specifies the amount of time after which an unresponsive (dead) server will be checked if it is working.
		/// </summary>
		/// <returns>The value of the dead timeout. The default is 2 minutes.</returns>
		[ConfigurationProperty("deadTimeout", IsRequired = false, DefaultValue = "00:02:00"), PositiveTimeSpanValidator, TypeConverter(typeof(InfiniteTimeSpanConverter))]
		public TimeSpan DeadTimeout
		{
			get { return (TimeSpan)base["deadTimeout"]; }
			set { base["deadTimeout"] = value; }
		}

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		protected override void PostDeserialize()
		{
			base.PostDeserialize();

			if(this.MinPoolSize > this.MaxPoolSize)
				throw new ConfigurationErrorsException("maxPoolSize must be larger than minPoolSize.");
		}

		#region [ ISocketPoolConfiguration     ]

		int ISocketPoolConfiguration.MinPoolSize
		{
			get { return this.MinPoolSize; }
			set { this.MinPoolSize = value; }
		}

		int ISocketPoolConfiguration.MaxPoolSize
		{
			get { return this.MaxPoolSize; }
			set { this.MaxPoolSize = value; }
		}

		TimeSpan ISocketPoolConfiguration.ConnectionTimeout
		{
			get { return this.ConnectionTimeout; }
			set { this.ConnectionTimeout = value; }
		}

		TimeSpan ISocketPoolConfiguration.DeadTimeout
		{
			get { return this.DeadTimeout; }
			set { this.DeadTimeout = value; }
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
