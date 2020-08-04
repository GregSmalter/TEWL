using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Tewl.InputValidation;

namespace Tewl.IO {
	public class TextBasedTabularDataParser: TabularDataParser {
		internal virtual CsvParsedLine Parse( string line ) => throw new NotImplementedException( "Parsers must have a specific implementation of Parse." );

		protected FileReader fileReader;

		/// <summary>
		/// For every line (after headerRowsToSkip) in the file with the given path, calls the line handling method you pass.
		/// The validationErrors collection will hold all validation errors encountered during the processing of all lines.
		/// When processing extremely large data sets, accumulating validationErrors in one collection may result in high memory
		/// usage. To avoid
		/// this, use the overload without this collection.
		/// Each line handler method will be given a fresh validator to do its work with.
		/// </summary>
		public override void ParseAndProcessAllLines( LineProcessingMethod lineHandler, ICollection<ValidationError> validationErrors ) {
			fileReader.ExecuteInStreamReader(
				delegate( StreamReader reader ) {
					IDictionary columnHeadersToIndexes = null;

					// This skips the header row and creates a name to index map out of it. 
					if( hasHeaderRow )
						columnHeadersToIndexes = buildColumnHeadersToIndexesDictionary( reader.ReadLine() );

					// GMS NOTE: It may be time to just remove this feature. It seems like only FixedWidth uses it (can be created with a non-zero value) and there are probably better ways to do it for whatever program needed that. 
					// This is a bit misleading. HeaderRowsToSkip will be 0 even when we've skipped the header row due to hasHeadRow, above. 
					for( var i = 0; i < headerRowsToSkip; i++ )
						reader.ReadLine();

					string line;
					for( var lineNumber = HeaderRows + 1; ( line = reader.ReadLine() ) != null; lineNumber++ ) {
						NonHeaderRows++;
						var parsedLine = Parse( line );
						if( parsedLine.ContainsData ) {
							RowsContainingData++;
							parsedLine.LineNumber = lineNumber;
							parsedLine.ColumnHeadersToIndexes = columnHeadersToIndexes;
							var validator = new Validator();
							lineHandler( validator, parsedLine );
							if( validator.ErrorsOccurred ) {
								if( validationErrors != null ) {
									foreach( var error in validator.Errors )
										validationErrors.Add( new ValidationError( "Line " + lineNumber, error.UnusableValueReturned, error.Message ) );
								}
							}
							else
								RowsWithoutValidationErrors++;
						}
					}
				} );
		}

		private IDictionary buildColumnHeadersToIndexesDictionary( string headerLine ) {
			IDictionary columnHeadersToIndexes = new ListDictionary();
			var index = 0;
			foreach( var columnHeader in Parse( headerLine ).Fields ) {
				columnHeadersToIndexes[ columnHeader.ToLower() ] = index;
				index++;
			}

			return columnHeadersToIndexes;
		}
	}
}