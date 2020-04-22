using System;
using System.Collections.Generic;
using System.Linq;

namespace Tewl.Tools {
	/// <summary>
	/// Extension methods and other static tools pertaining to Enum types.
	/// </summary>
	public static class EnumTools {
		/// <summary>
		/// Converts this string to a given Enum value. Case sensitive.
		/// This method does not enforce valid Enum values.
		/// </summary>
		/// C# doesn't allow constraining the value to an Enum
		public static T ToEnum<T>( this string s ) => (T)Enum.Parse( typeof( T ), s );

		/// <summary>
		/// Gets the values of the specified enumeration type.
		/// </summary>
		// C# doesn't allow constraining the type to an Enum.
		public static IEnumerable<T> GetValues<T>() => Enum.GetValues( typeof( T ) ).Cast<T>();

		/// <summary>
		/// Looks for <see cref="EnglishAttribute" /> and if available, returns its value.
		/// Otherwise, returns the name of the enum value after applying <see cref="StringTools.CamelToEnglish(string)" />.
		/// </summary>
		public static string ToEnglish( this Enum e ) {
			var name = e.GetAttribute<EnglishAttribute>();
			return name != null ? name.English : Enum.GetName( e.GetType(), e ).CamelToEnglish();
		}
	}
}