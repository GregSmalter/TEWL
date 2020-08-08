using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing {
	/// <summary>
	/// Use this to process several lines of any type of tabular data, such as CSVs, fixed-width data files, or Excel files.
	/// </summary>
	[ PublicAPI ]
	public class TabularDataParser {
		/// <summary>
		/// Method that knows how to process a line from a particular file.  The validator is new for each row and has no errors,
		/// initially.
		/// </summary>
		public delegate void LineProcessingMethod( Validator validator, TabularDataParsedLine line );

		/// <summary>
		/// Header rows to skip, shared by all parsers.
		/// </summary>
		protected int headerRowsToSkip;

		/// <summary>
		/// True if there is a header row. Also implies header rows to skip is 1.
		/// </summary>
		protected bool hasHeaderRow;

		/// <summary>
		/// The number of rows in the file, not including the header rows that were skipped with headerRowsToSkip or hasHeaderRows
		/// = true.
		/// This is the number of rows in the file that were parsed.
		/// This properly only has meaning after ParseAndProcessAllLines has been called.
		/// </summary>
		public int NonHeaderRows { get; protected set; }

		/// <summary>
		/// The number of header rows in the file. This is equal to 1 if hasHeaderRow was passed as true, or equal to
		/// headerRowsToSkip otherwise.
		/// </summary>
		public int HeaderRows => hasHeaderRow ? 1 : headerRowsToSkip;

		/// <summary>
		/// The total number of rows in the file, including any header rows. This properly only has meaning after
		/// ParseAndProcessAllLines has been called.
		/// </summary>
		public int TotalRows => HeaderRows + NonHeaderRows;

		/// <summary>
		/// The number of rows in the file with at least one non-blank field.
		/// This properly only has meaning after ParseAndProcessAllLines has been called.
		/// This is the number of rows in that file that were processed (the lineHandler callback was performed).
		/// </summary>
		public int RowsContainingData { get; protected set; }

		/// <summary>
		/// The number of rows in the file that were processed without encountering any validation errors.
		/// This properly only has meaning after ParseAndProcessAllLines has been called.
		/// </summary>
		public int RowsWithoutValidationErrors { get; protected set; }

		/// <summary>
		/// The number of rows in the file that did encounter validation errors when processed.
		/// This properly only has meaning after ParseAndProcessAllLines has been called.
		/// </summary>
		public int RowsWithValidationErrors => RowsContainingData - RowsWithoutValidationErrors;

		/// <summary>
		/// Constructs a tabular data parser. Empty. 
		/// </summary>
		protected TabularDataParser() { }

		/// <summary>
		/// Creates a parser designed to parse a file with fixed data column widths. Specify the starting position of each column
		/// (using one-based column index).
		/// Characters that take up more than 1 unit of width, such as tabs, can cause problems here.
		/// </summary>
		public static TabularDataParser CreateForFixedWidthFile( string filePath, int headerRowsToSkip, params int[] columnStartPositions ) =>
			FixedWidthParser.CreateWithFilePath( filePath, headerRowsToSkip, columnStartPositions );

		/// <summary>
		/// Creates a parser designed to parse a CSV file.  Passing true for hasHeaderRow will result in the first row being used
		/// to map
		/// header names to column indices.  This will allow you to access fields using the header name in addition to the column
		/// index.
		/// </summary>
		public static TabularDataParser CreateForCsvFile( string filePath, bool hasHeaderRow ) => CsvLineParser.CreateWithFilePath( filePath, hasHeaderRow );

		/// <summary>
		/// Creates a parser designed to parse a CSV file.  Passing true for hasHeaderRow will result in the first row being used
		/// to map
		/// header names to column indices.  This will allow you to access fields using the header name in addition to the column
		/// index.
		/// </summary>
		public static TabularDataParser CreateForCsvFile( Stream stream, bool hasHeaderRow ) => CsvLineParser.CreateWithStream( stream, hasHeaderRow );

		/// <summary>
		/// Assumes header row. Fields will always be accessible by name.
		/// </summary>
		public static TabularDataParser CreateForExcelFile( Stream stream ) => new ExcelParser( stream ) { hasHeaderRow = true };

		/// <summary>
		/// Assumes header row. Fields will always be accessible by name.
		/// </summary>
		public static TabularDataParser CreateForExcelFile( string filePath ) => new ExcelParser( filePath ) { hasHeaderRow = true };

		/// <summary>
		/// For every line (after headerRowsToSkip) in the file with the given path, calls the line handling method you pass.
		/// Each line handler method will be given a fresh validator to do its work with.
		/// </summary>
		public void ParseAndProcessAllLines( LineProcessingMethod lineHandler ) {
			ParseAndProcessAllLines( lineHandler, null );
		}

		/// <summary>
		/// For every line (after headerRowsToSkip) in the file with the given path, calls the line handling method you pass.
		/// Each line handler method will be given a fresh validator to do its work with.
		/// </summary>
		public virtual void ParseAndProcessAllLines( LineProcessingMethod lineHandler, ICollection<ValidationError> validationErrors ) {
			throw new NotImplementedException( "Each parser must have a specific implementation of parse and process all lines." );
		}
	}
}