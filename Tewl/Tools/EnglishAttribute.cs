using System;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Apply this attribute to the value of an Enum.
	/// </summary>
	[ PublicAPI ]
	public class EnglishAttribute: Attribute {
		/// <summary>
		/// An English representation of an enum value. Can include spaces. Use this to have more descriptive English conversations
		/// of Enum values.
		/// </summary>
		public readonly string English;

		/// <summary>
		/// Creates an attribute with the given English representation.
		/// </summary>
		public EnglishAttribute( string english ) => English = english;
	}
}