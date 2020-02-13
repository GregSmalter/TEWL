#nullable enable

using System.Collections.Generic;

namespace Tewl.Tools {
	/// <summary>
	/// Dictionary extensions.
	/// </summary>
	public static class DictionaryTools {
		/// <summary>
		/// Tries to get dictionary value. Returns null if the <paramref name="key" /> was not found.
		/// Note that this only makes sense if you don't expect to receive null for a key found in the dictionary.
		/// </summary>
		public static V TryGetValue<K, V>( this Dictionary<K, V> dic, K key ) where V: class => dic.TryGetValue( key, out var v ) ? v : null;

		/// <summary>
		/// Tries to get dictionary value. Returns null if the <paramref name="key" /> was not found.
		/// Note that this only makes sense if you don't expect to receive null for a key found in the dictionary.
		/// </summary>
		public static V? TryGetValueNullable<K, V>( this Dictionary<K, V> dic, K key ) where V: struct => dic.TryGetValue( key, out var v ) ? v : (V?)null;

		/// <summary>
		/// Tries to get dictionary value. Returns <paramref name="valueIfNotFound" /> if the <paramref name="key" /> was not
		/// found.
		/// </summary>
		public static V TryGetValue<K, V>( this Dictionary<K, V> dic, K key, V valueIfNotFound ) where V: struct => dic.TryGetValue( key, out var v ) ? v : valueIfNotFound;
	}
}