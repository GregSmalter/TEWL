namespace Tewl.Tools;

/// <summary>
/// Extension methods and other static methods pertaining to things of type object.
/// </summary>
[ PublicAPI ]
public static class ObjectTools {
	/// <summary>
	/// Transforms the underlying value of this nullable object using the specified selector, if an underlying value exists.
	/// </summary>
	public static DestinationType? ToNewUnderlyingValue<SourceType, DestinationType>( this SourceType? value, Func<SourceType, DestinationType> valueSelector )
		where SourceType: struct where DestinationType: struct =>
		value.HasValue ? valueSelector( value.Value ) : null;

	/// <summary>
	/// Returns o.ToString() unless o is null. In this case, returns either null (if nullToEmptyString is false) or the empty
	/// string (if nullToEmptyString is true).
	/// </summary>
	public static string? ObjectToString( this object? o, bool nullToEmptyString ) => o is not null ? o.ToString() : nullToEmptyString ? string.Empty : null;
}