namespace Tewl.Tools {
	public static class BoolTools {
		/// <summary>
		/// Returns "Yes" if this is true, "No" if it is false, and the empty string if it is null.
		/// </summary>
		public static string ToYesOrNo( this bool? b ) {
			return b.HasValue ? b.Value.ToYesOrNo() : "";
		}

		/// <summary>
		/// Returns "Yes" if this is true and "No" otherwise.
		/// </summary>
		public static string ToYesOrNo( this bool b ) {
			return b ? "Yes" : "No";
		}

		/// <summary>
		/// Returns "Yes" if this is true and the empty string otherwise.
		/// </summary>
		public static string ToYesOrEmpty( this bool b ) {
			return b ? "Yes" : "";
		}

		/// <summary>
		/// Converts a boolean into a decimal for storage in Oracle.
		/// </summary>
		public static decimal BooleanToDecimal( this bool b ) {
			return b ? 1 : 0;
		}

	}
}