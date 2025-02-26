using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing;

/// <summary>
/// Data parser for text-based file formats of tabular data, such as CSV and fixed-width.
/// </summary>
internal abstract class TextBasedTabularDataParser: TabularDataParser {
	protected FileReader? fileReader;

	protected abstract IReadOnlyList<string> parseLine( string? line );

	public override void ParseAndProcessAllLines(
		LineProcessingMethod lineHandler, ICollection<ValidationError> validationErrors, bool disableLineProcessingErrorAccumulation = false ) {
		fileReader!.ExecuteInStreamReader(
			delegate( StreamReader reader ) {
				IReadOnlyDictionary<string, int>? columnHeadersToIndexes = null;
				if( requiredColumns is not null ) {
					// This skips the header row and creates a name to index map out of it.
					columnHeadersToIndexes = buildColumnHeadersToIndexesDictionary( reader.ReadLine() );

					var missingColumns = requiredColumns.Where( i => !columnHeadersToIndexes.ContainsKey( i.ToLower() ) ).Materialize();
					if( missingColumns.Any() ) {
						var columnList = StringTools.GetEnglishListPhrase( missingColumns, true );
						var singularize = missingColumns.Count == 1;
						validationErrors.Add(
							new ValidationError(
								"Header line",
								false,
								$"The required {( singularize ? "column" : "columns" )} {columnList} {( singularize ? "is" : "are" )} missing." +
								( columnHeadersToIndexes.Count == 1
									  ? " Also, only a single column was detected, so please confirm that fields are separated by commas and not semicolons."
									  : "" ) ) );
					}
				}

				// GMS NOTE: It may be time to just remove this feature. It seems like only FixedWidth uses it (can be created with a non-zero value) and there are probably better ways to do it for whatever program needed that. 
				// This is a bit misleading. HeaderRowsToSkip will be 0 even when we've skipped the header row due to hasHeadRow, above. 
				for( var i = 0; i < headerRowsToSkip; i++ )
					reader.ReadLine();

				for( var lineNumber = HeaderRows + 1; reader.ReadLine() is {} line; lineNumber++ ) {
					NonHeaderRows++;
					var parsedLine = new TextBasedParsedLine( columnHeadersToIndexes, lineNumber, parseLine( line ) );
					if( parsedLine.ContainsData ) {
						RowsContainingData++;
						var validator = new Validator();
						lineHandler( validator, parsedLine );
						if( !validator.ErrorsOccurred )
							RowsWithoutValidationErrors++;
						else if( !disableLineProcessingErrorAccumulation )
							foreach( var error in validator.Errors )
								validationErrors.Add( new ValidationError( "Line " + lineNumber, error.UnusableValueReturned, error.Message ) );
					}
				}
			} );
	}

	private IReadOnlyDictionary<string, int> buildColumnHeadersToIndexesDictionary( string? headerLine ) {
		var columnHeadersToIndexes = new Dictionary<string, int>();
		var index = 0;
		foreach( var columnHeader in parseLine( headerLine ) ) {
			columnHeadersToIndexes[ columnHeader.ToLower() ] = index;
			index++;
		}

		return columnHeadersToIndexes;
	}
}