namespace Tewl;

[ PublicAPI ]
public static class NewlineConstants {
	/// <summary>
	/// The Unix newline sequence (U+000A LINE FEED). Use this as the default line ending for cross-platform text.
	/// </summary>
	public const string Newline = "\n";

	/// <summary>
	/// The Windows newline sequence (U+000D CARRIAGE RETURN followed by U+000A LINE FEED).
	/// </summary>
	public const string WindowsNewline = "\r\n";
}