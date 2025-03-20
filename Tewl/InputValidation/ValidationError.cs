namespace Tewl.InputValidation;

/// <summary>
/// A validation error.
/// </summary>
public class ValidationError {
	internal static ValidationError Custom( ValidationErrorType errorType, string errorMessage ) => new( errorType, errorMessage );

	internal static ValidationError Invalid() => new( ValidationErrorType.Invalid, "Please enter a valid {0}." );

	internal static ValidationError Empty() => new( ValidationErrorType.Empty, "Please enter the {0}." );

	internal static ValidationError TooSmall( object min, object max ) =>
		new( ValidationErrorType.TooLong, "The {0} must be between " + min + " and " + max + " (inclusive)." );

	internal static ValidationError TooLarge( object min, object max ) =>
		new( ValidationErrorType.TooLarge, "The {0} must be between " + min + " and " + max + " (inclusive)." );

	/// <summary>
	/// Gets the error type.
	/// </summary>
	public ValidationErrorType Type { get; }

	private readonly string errorMessage;

	private ValidationError( ValidationErrorType type, string message ) {
		Type = type;
		errorMessage = message;
	}

	/// <summary>
	/// Returns the standard message for this error.
	/// </summary>
	/// <param name="valueName">The name of the result value. If this is used to start a sentence, it will be automatically capitalized.</param>
	public string GetMessage( string valueName ) => string.Format( errorMessage, valueName );
}