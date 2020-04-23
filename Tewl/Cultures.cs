using System.Globalization;
using JetBrains.Annotations;

namespace Tewl {
	/// <summary>
	/// Cultures supported by our framework.
	/// </summary>
	[ PublicAPI ]
	public static class Cultures {
		/// <summary>
		/// English (United States)
		/// </summary>
		public static CultureInfo EnglishUnitedStates => CultureInfo.GetCultureInfo( "en-US" );

		/// <summary>
		/// Spanish (Spain)
		/// </summary>
		public static CultureInfo SpanishSpain => CultureInfo.GetCultureInfo( "es-ES" );
	}
}