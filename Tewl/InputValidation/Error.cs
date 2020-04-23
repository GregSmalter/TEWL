using JetBrains.Annotations;

namespace Tewl.InputValidation {
	/// <summary>
	/// Represents a validation error.
	/// </summary>
	[ PublicAPI ]
	public class Error {
		internal Error( string message, bool unusableValueReturned ) {
			Message = message;
			UnusableValueReturned = unusableValueReturned;
		}

		/// <summary>
		/// The error message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Returns true if the error resulted in an unusable value being returned.
		/// </summary>
		public bool UnusableValueReturned { get; }
	}
}