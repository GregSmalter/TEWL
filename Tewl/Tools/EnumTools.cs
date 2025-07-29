namespace Tewl.Tools;

/// <summary>
/// Extension methods and other static tools pertaining to Enum types.
/// </summary>
[ PublicAPI ]
public static class EnumTools {
	/// <summary>
	/// Converts this string to a given Enum value. Case sensitive.
	/// This method does not enforce valid Enum values.
	/// </summary>
	public static T ToEnum<T>( this string s ) where T: Enum => (T)Enum.Parse( typeof( T ), s );

	/// <summary>
	/// Gets the values of the specified enumeration type.
	/// </summary>
	public static IEnumerable<T> GetValues<T>() where T: Enum => Enum.GetValues( typeof( T ) ).Cast<T>();

	/// <summary>
	/// Looks for <see cref="EnglishAttribute" /> and if available, returns its value.
	/// Otherwise, returns the name of the enum value after applying <see cref="StringTools.CamelToEnglish(string)" />.
	/// </summary>
	public static string ToEnglish( this Enum e ) {
		var name = e.GetAttribute<EnglishAttribute>();
		return name != null ? name.English : e.ToString().CamelToEnglish();
	}
}