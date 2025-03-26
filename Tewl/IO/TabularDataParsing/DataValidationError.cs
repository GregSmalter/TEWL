namespace Tewl.IO.TabularDataParsing;

/// <summary>
/// Holds information about a validation error generated as a result of processing a parsed line.
/// </summary>
[ PublicAPI ]
public class DataValidationError {
	/// <summary>
	/// An explanation of the place in the original data that caused the error.  For example, "Line 32".
	/// </summary>
	public string ErrorSource { get; set; }

	/// <summary>
	/// True if this is a fatal error.
	/// </summary>
	public bool IsFatal { get; }

	/// <summary>
	/// The error message.
	/// </summary>
	public string ErrorMessage { get; }

	/// <summary>
	/// Creates a validation error that occurred when processing the given line number.
	/// Error source is an explanation of the place in the original data that caused the error.  For example, "Line 32".
	/// </summary>
	public DataValidationError( string errorSource, bool isFatal, string errorMessage ) {
		ErrorSource = errorSource;
		ErrorMessage = errorMessage;
		IsFatal = isFatal;
	}
}