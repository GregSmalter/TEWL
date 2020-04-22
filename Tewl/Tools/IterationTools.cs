using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Helpful functions based-on iteration.
	/// </summary>
	[ PublicAPI ]
	public static class IterationTools {
		/// <summary>
		/// Passes <paramref name="rowsAtaTime" /> items from <paramref name="rows" /> to <paramref name="someRowsAction" /> at a
		/// time.
		/// Use this to query a large amount of rows in a database in pieces.
		/// </summary>
		/// <param name="rows">Rows to pass to <paramref name="someRowsAction" /></param>
		/// <param name="rowsAtaTime">How many items to pass to <paramref name="someRowsAction" /> at a time.</param>
		/// <param name="someRowsAction">
		/// Processes <paramref name="rowsAtaTime" /> items from the <paramref name="rows" /> at a
		/// time.
		/// </param>
		public static void IterateInPieces<T>( IEnumerable<T> rows, int rowsAtaTime, Action<IEnumerable<T>> someRowsAction ) {
			var length = rows.Count();
			var i = 0;
			while( i < length ) {
				someRowsAction( rows.Skip( i ).Take( rowsAtaTime ) );
				i += rowsAtaTime;
			}
		}
	}
}