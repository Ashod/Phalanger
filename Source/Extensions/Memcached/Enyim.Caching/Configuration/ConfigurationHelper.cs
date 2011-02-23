using System;

namespace Enyim.Caching.Configuration
{
    /// <summary>
    /// Helper methods.
    /// </summary>
	public static class ConfigurationHelper
	{
        /// <summary>
        /// Check if type is of given interface. Throws an exception if not.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">Type does not implement given interface.</exception>
		public static void CheckForInterface(Type type, Type interfaceType)
		{
			if (Array.IndexOf<Type>(type.GetInterfaces(), interfaceType) == -1)
				throw new System.Configuration.ConfigurationErrorsException("The type " + type.AssemblyQualifiedName + " must implement " + interfaceType.AssemblyQualifiedName);
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
