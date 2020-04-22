using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Tools for the <see cref="Random" /> class. Not like, a random set of tools.
	/// </summary>
	[ PublicAPI ]
	public static class RandomTools {
		/// <summary>
		/// Returns a random string with the given min and max lengths (both inclusive). The default minLength is 0 (empty string
		/// can be returned) and
		/// the default max length is 8.
		/// </summary>
		public static string GetRandomString( this Random r, int minLength = 0, int maxLength = 8 ) {
			if( minLength > maxLength )
				throw new ArgumentException( $"{nameof(minLength)} cannot be greater than {nameof(maxLength)}." );

			var length = r.NextInt( minLength, maxLength );
			var randomString = new StringBuilder();
			for( var i = 0; i < length; i++ )
				randomString.Append( r.GetRandomLetter() );
			return randomString.ToString();
		}

		/// <summary>
		/// Returns a random lowercase letter from the 26-letter alphabet.
		/// </summary>
		public static char GetRandomLetter( this Random r ) {
			const string letters = "abcdefghiklmnopqrstuvwxyz";
			return letters[ r.Next( letters.Length ) ];
		}

		/// <summary>
		/// Returns default if the enumeration has no elements.
		/// </summary>
		public static T GetRandomElement<T>( this IEnumerable<T> enumeration, Random r ) => enumeration.ElementAtOrDefault( r.Next( enumeration.Count() ) );

		/// <summary>
		/// Returns default if the enumeration has no elements.
		/// </summary>
		public static T GetRandomElement<T>( this IEnumerable<T> enumeration ) => enumeration.ElementAtOrDefault( new Random().Next( enumeration.Count() ) );

		/// <summary>
		/// Generates int. Min and max are inclusive.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="min">Inclusive.</param>
		/// <param name="max">Inclusive.</param>
		/// <returns></returns>
		public static int NextInt( this Random r, int min = 0, int max = int.MaxValue ) => r.Next( max - min ) + min + 1;

		/// <summary>
		/// Returns a random bool.
		/// </summary>
		public static bool FlipCoin( this Random r ) => r.Next( 2 ) == 1;
	}
}