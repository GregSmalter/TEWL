namespace Tewl.InputValidation {
	internal class ValidationResult {
		private string errorMessage = "";

		private ValidationResult() { }

		public string GetErrorMessage( string subject ) => string.Format( errorMessage, subject );

		public ErrorCondition ErrorCondition { get; private set; } = ErrorCondition.NoError;

		public static ValidationResult Custom( ErrorCondition errorCondition, string errorMessage ) => new ValidationResult { ErrorCondition = errorCondition, errorMessage = errorMessage };

		public static ValidationResult NoError() => new ValidationResult();

		public static ValidationResult Invalid() => new ValidationResult { ErrorCondition = ErrorCondition.Invalid, errorMessage = "Please enter a valid {0}." };

		public static ValidationResult Empty() => new ValidationResult { ErrorCondition = ErrorCondition.Empty, errorMessage = "Please enter the {0}." };

		public static ValidationResult TooSmall( object min, object max ) => new ValidationResult { ErrorCondition = ErrorCondition.TooLong, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };

		public static ValidationResult TooLarge( object min, object max ) => new ValidationResult { ErrorCondition = ErrorCondition.TooLarge, errorMessage = "The {0} must be between " + min + " and " + max + " (inclusive)." };
	}
}