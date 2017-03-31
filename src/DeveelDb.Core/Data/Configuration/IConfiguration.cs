// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.Collections.Generic;

namespace Deveel.Data.Configuration {
	/// <summary>
	/// Defines the contract for the configuration node of a component within
	/// the system or of the system itself.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Configurations can be structured in <c>nodes</c> of a tree,
	/// it is possible to define child sections to a parent, and a descending
	/// order will be used to resolve a key and value of a setting, if the current 
	/// node does not define it by itself.
	/// </para>
	/// </remarks>
	public interface IConfiguration : IEnumerable<KeyValuePair<string, object>> {
		/// <summary>
		/// Enumerates the keys that can be obtained by the object
		/// </summary>
		/// <returns>
		/// Returns an enumeration of <see cref="string"/> representing the
		/// keys that are accessible from this object
		/// </returns>
		IEnumerable<string> Keys { get; }
		
		/// <summary>
		/// Sets a given value for a key defined by this object.
		/// </summary>
		/// <param name="key">The key to set the value for, that was defined before.</param>
		/// <param name="value">The value to set.</param>
		/// <remarks>
		/// <para>
		/// If the given <paramref name="key"/> was not previously defined,
		/// this method will add the key at this level of configuration
		/// </para>
		/// <para>
		/// Setting a value for a given <paramref name="key"/> that was already
		/// defined by a parent object will override that value: a subsequent call
		/// to <see cref="GetValue"/> will return the current value of the setting,
		/// without removing the parent value setting.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// If the given <paramref name="key"/> is <c>null</c>.
		/// </exception>
		void SetValue(string key, object value);

		/// <summary>
		/// Gets a configuration setting for the given key.
		/// </summary>
		/// <param name="key">The key that identifies the setting to retrieve.</param>
		/// <returns>
		/// Returns a configuration value if defined by the provided key, or <c>null</c>
		/// if the key was not found in this or in the parent context.
		/// </returns>
		object GetValue(string key);

		/// <summary>
		/// Adds a child configuration to this one
		/// </summary>
		/// <param name="key">The key used to identify the child configuration</param>
		/// <param name="child">The configuration object</param>
		/// <remarks>
		/// When this method returns the <see cref="Parent"/> of the
		/// passed <paramref name="child"/> will be set to this object
		/// </remarks>
		void AddChild(string key, IConfiguration child);

		IEnumerable<KeyValuePair<string, IConfiguration>> GetChildren();
	}
}