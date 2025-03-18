namespace Tewl.InputValidation;

internal class ValidationError {
	public static ValidationError Custom( ErrorCondition errorCondition, string errorMessage ) =>
		new() { ErrorCondition = errorCondition, errorMessage = errorMessage };

	public static ValidationError NoError() => new();

	public static ValidationError Invalid() => new() { ErrorCondition = ErrorCondition.Invalid, errorMessage = "Please enter a valid {0}." };

	public static ValidationError Empty() => new() { ErrorCondition = ErrorCondition.Empty, errorMessage = "Please enter the {0}." };

	public static ValidationError TooSmall( object min, object max ) =>
		new() { ErrorCondition = ErrorCondition.TooLong, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };

	public static ValidationError TooLarge( object min, object max ) =>
		new() { ErrorCondition = ErrorCondition.TooLarge, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };

	public ErrorCondition ErrorCondition { get; private init; } = ErrorCondition.NoError;
	private string errorMessage = "";

	private ValidationError() {}

	public string GetMessage( string subject ) => string.Format( errorMessage, subject );
}