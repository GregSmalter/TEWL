namespace Tewl;

/// <summary>
/// A trusted HTML string.
/// </summary>
[ PublicAPI ]
public sealed class TrustedHtmlString {
	/// <summary>
	/// Gets the HTML.
	/// </summary>
	public string Html { get; }

	/// <summary>
	/// Creates a trusted HTML string.
	/// </summary>
	/// <param name="html">Do not pass null.</param>
	public TrustedHtmlString( string html ) {
		Html = html;
	}
}