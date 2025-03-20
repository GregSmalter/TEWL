namespace Tewl.InputValidation;

/// <summary>
/// A validation error.
/// </summary>
public class ValidationError {
	internal static ValidationError Custom( ErrorCondition errorCondition, string errorMessage ) =>
		new() { ErrorCondition = errorCondition, errorMessage = errorMessage };

	internal static ValidationError NoError() => new();

	internal static ValidationError Invalid() => new() { ErrorCondition = ErrorCondition.Invalid, errorMessage = "Please enter a valid {0}." };

	internal static ValidationError Empty() => new() { ErrorCondition = ErrorCondition.Empty, errorMessage = "Please enter the {0}." };

	internal static ValidationError TooSmall( object min, object max ) =>
		new() { ErrorCondition = ErrorCondition.TooLong, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };

	internal static ValidationError TooLarge( object min, object max ) =>
		new() { ErrorCondition = ErrorCondition.TooLarge, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };

	/// <summary>
	/// Gets the error type.
	/// </summary>
	public ErrorCondition ErrorCondition { get; private init; } = ErrorCondition.NoError;

	private string errorMessage = "";

	private ValidationError() {}

	/// <summary>
	/// Returns the standard message for this error.
	/// </summary>
	/// <param name="valueName">The name of the result value. If this is used to start a sentence, it will be automatically capitalized.</param>
	public string GetMessage( string valueName ) => string.Format( errorMessage, valueName );
}