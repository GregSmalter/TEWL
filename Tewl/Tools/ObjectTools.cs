using System;

namespace Tewl.Tools {
	/// <summary>
	/// Extension methods and other static methods pertaining to things of type object.
	/// </summary>
	public static class ObjectTools {
		/// <summary>
		/// Returns o.ToString() unless o is null. In this case, returns either null (if nullToEmptyString is false) or the empty
		/// string (if nullToEmptyString is true).
		/// </summary>
		public static string ObjectToString( this object o, bool nullToEmptyString ) {
			if( o != null )
				return o.ToString();
			return nullToEmptyString ? String.Empty : null;
		}
	}
}