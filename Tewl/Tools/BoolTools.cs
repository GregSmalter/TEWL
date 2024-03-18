using JetBrains.Annotations;

namespace Tewl.Tools;

/// <summary>
/// Extension methods and other static tools pertaining to boolean types and boolean/string conversation (but not
/// validation).
/// </summary>
[ PublicAPI ]
public static class BoolTools {
	/// <summary>
	/// Returns "Yes" if this is true, "No" if it is false, and the empty string if it is null.
	/// </summary>
	public static string ToYesOrNo( this bool? b ) => b.HasValue ? b.Value.ToYesOrNo() : "";

	/// <summary>
	/// Returns "Yes" if this is true and "No" otherwise.
	/// </summary>
	public static string ToYesOrNo( this bool b ) => b ? "Yes" : "No";

	/// <summary>
	/// Returns "Yes" if this is true and the empty string otherwise.
	/// </summary>
	public static string ToYesOrEmpty( this bool b ) => b ? "Yes" : "";
}