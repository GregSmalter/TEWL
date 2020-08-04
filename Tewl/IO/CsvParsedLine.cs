using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tewl.Tools;

// GMS NOTE: Rename to TextBasedTabularParsedLine or something .This is used by fixed width as well. Deliited?

namespace Tewl.IO {
	/// <summary>
	/// Represents a line of text from a CSV file that has been parsed into fields that
	/// are accessible through the indexers of this object.
	/// </summary>
	[PublicAPI]
	internal class CsvParsedLine : TabularDataParsedLine {
		private IDictionary<string, int> columnHeadersToIndexes;
		private int? lineNumber;

		/// <summary>
		/// Returns true if any field on this line has a non-empty, non-whitespace value.
		/// </summary>
		public bool ContainsData { get; }

		/// <summary>
		/// Returns the line number from the source document that this parsed line was created from.
		/// </summary>
		public int LineNumber {
			get {
				if( lineNumber.HasValue )
					return lineNumber.Value;
				throw new ApplicationException( "Line number has not been initialized and has no meaning." );
			}
			internal set => lineNumber = value;
		}

		internal IDictionary<string, int> ColumnHeadersToIndexes { set => columnHeadersToIndexes = value ?? new Dictionary<string, int>(); }

		internal CsvParsedLine( List<string> fields ) {
			this.fields = fields;
			ContainsData = false;
			foreach( var field in fields ) {
				if( !field.IsNullOrWhiteSpace() ) {
					ContainsData = true;
					break;
				}
			}
		}

		internal List<string> Fields { get; }

		/// <summary>
		/// Returns the value of the field with the given column index.
		/// Gracefully return empty string when over-indexed.  This prevents problems with files that have no value in the last column.
		/// </summary>
		public string this[ int index ] {
			get {
				if( index >= Fields.Count )
					return "";
				return Fields[ index ];
			}
		}

		/// <summary>
		/// Returns the value of the field with the given column name.
		/// </summary>
		public string this[ string columnName ] {
			get {
				if( columnHeadersToIndexes.Count == 0 )
					throw new InvalidOperationException( "The CSV parser returning this CsvLine was not created with a headerLine with which to populate column names." );

				if( columnName == null )
					throw new ArgumentException( "Column name cannot be null." );

				if( !columnHeadersToIndexes.TryGetValue( columnName.ToLower(), out var index ) ) {
					var keys = "";
					foreach( var key in columnHeadersToIndexes.Keys )
						keys += key + ", ";
					throw new ArgumentException( "Column '" + columnName + "' does not exist.  The columns are: " + keys );
				}

				return this[ index ];
			}
		}

		/// <summary>
		/// Returns a comma-delimited list of fields.
		/// </summary>
		public override string ToString() {
			var text = "";
			foreach( var field in Fields )
				text += ", " + field;
			return text.TruncateStart( text.Length - 2 );
		}

		/// <summary>
		/// Returns true if the line contains the given field.
		/// </summary>
		public bool ContainsField( string fieldName ) => columnHeadersToIndexes.Keys.Contains( fieldName.ToLower() );
	}
}