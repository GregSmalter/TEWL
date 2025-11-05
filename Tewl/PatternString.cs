using System.Text.RegularExpressions;

namespace Tewl;

/// <summary>
/// A search pattern string.
/// </summary>
[ PublicAPI ]
public sealed class PatternString: IEquatable<PatternString> {
	/// <summary>
	/// Gets the pattern.
	/// </summary>
	public string Pattern { get; }

	/// <summary>
	/// Creates a search pattern string.
	/// </summary>
	public PatternString( string pattern ) {
		Pattern = pattern;
	}

	/// <summary>
	/// Returns true if this pattern matches the specified text. Always returns true if the pattern is empty. Ignores case.
	/// <para>Examples: Given the text “Example”, this method returns true for the patterns “ex” and “example”.</para>
	/// </summary>
	/// <param name="text"></param>
	/// <param name="requireFullMatch">Prevents the pattern “ge” from matching “General Mills”. Only “general mills” will match.</param>
	public bool Matches( string text, bool requireFullMatch = false ) {
		if( Pattern.Length == 0 )
			return true;

		var pat = Regex.Escape( Pattern );
		if( requireFullMatch )
			pat = $"^{pat}$";
		return Regex.IsMatch( text, pat, RegexOptions.IgnoreCase );
	}

#pragma warning disable CS1591
	public override bool Equals( object? obj ) => Equals( obj as PatternString );
	public bool Equals( PatternString? other ) => other is not null && Pattern.Equals( other.Pattern, StringComparison.Ordinal );
	public override int GetHashCode() => Pattern.GetHashCode();
#pragma warning restore CS1591
}