using System.Collections;
using JetBrains.Annotations;

namespace Tewl.InputValidation {
	/// <summary>
	/// Common data required to validate and build an error message for a piece of data.
	/// </summary>
	[ PublicAPI ]
	public class ValidationPackage {
		/// <summary>
		/// The subject of the error message, if one needs to be generated.
		/// </summary>
		public string Subject { get; }

		/// <summary>
		/// The map of ErrorConditions to error messages overrides.
		/// </summary>
		public IDictionary CustomMessages { get; }

		/// <summary>
		/// Returns the ErrorCondition resulting from the validation of the data
		/// associated with this package.
		/// </summary>
		public ErrorCondition ValidationResult { set; get; } = ErrorCondition.NoError;

		/// <summary>
		/// Create a new package with which to validate a piece of data.
		/// </summary>
		/// <param name="subject">The subject of the error message, if one needs to be generated.</param>
		/// <param name="customMessages">The map of ErrorConditions to error messages overrides. Use null for no overrides.</param>
		public ValidationPackage( string subject, IDictionary customMessages ) {
			Subject = subject;
			CustomMessages = customMessages;
		}

		/// <summary>
		/// Create a new package with which to validate a piece of data without
		/// specifying any custom messages overrides.
		/// </summary>
		/// <param name="subject">The subject of the error message, if one needs to be generated.</param>
		public ValidationPackage( string subject ): this( subject, null ) { }
	}
}