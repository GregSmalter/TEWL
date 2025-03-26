using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing;

/// <summary>
/// Data parser for text-based file formats of tabular data, such as CSV and fixed-width.
/// </summary>
internal abstract class TextBasedTabularDataParser: TabularDataParser {
	protected FileReader? fileReader;

	protected abstract IReadOnlyList<string> parseLine( string? line );

	public override void ParseAndProcessAllLines(
		LineProcessingMethod lineHandler, ICollection<DataValidationError> validationErrors, bool disableLineProcessingErrorAccumulation = false ) {
		fileReader!.ExecuteInStreamReader(
			reader => {
				IReadOnlyDictionary<string, int>? columnIndicesByName = null;
				if( requiredColumns is not null ) {
					// This skips the header row and creates a name to index map out of it.
					columnIndicesByName = parseLine( reader.ReadLine() ).Select( ( name, index ) => ( name, index ) ).ToDictionary( StringComparer.OrdinalIgnoreCase );

					var missingColumns = requiredColumns.Where( i => !columnIndicesByName.ContainsKey( i ) ).Materialize();
					if( missingColumns.Any() ) {
						var columnList = StringTools.GetEnglishListPhrase( missingColumns.Select( i => $"“{i}”" ), true );
						var singularize = missingColumns.Count == 1;
						validationErrors.Add(
							new DataValidationError(
								"Header line",
								false,
								$"The required {( singularize ? "column" : "columns" )} {columnList} {( singularize ? "is" : "are" )} missing." +
								( columnIndicesByName.Count == 1
									  ? " Also, only a single column was detected, so please confirm that fields are separated by commas and not semicolons."
									  : "" ) ) );
						return;
					}
				}

				// GMS NOTE: It may be time to just remove this feature. It seems like only FixedWidth uses it (can be created with a non-zero value) and there are probably better ways to do it for whatever program needed that. 
				// This is a bit misleading. HeaderRowsToSkip will be 0 even when we've skipped the header row due to hasHeadRow, above. 
				for( var i = 0; i < headerRowsToSkip; i++ )
					reader.ReadLine();

				for( var lineNumber = HeaderRows + 1; reader.ReadLine() is {} line; lineNumber++ ) {
					NonHeaderRows++;
					var parsedLine = new TextBasedParsedLine( columnIndicesByName, lineNumber, parseLine( line ) );
					if( parsedLine.ContainsData ) {
						RowsContainingData++;
						var validator = new Validator();
						lineHandler( validator, parsedLine );
						if( !validator.ErrorsOccurred )
							RowsWithoutValidationErrors++;
						else if( !disableLineProcessingErrorAccumulation )
							foreach( var error in validator.Errors )
								validationErrors.Add( new DataValidationError( "Line " + lineNumber, error.UnusableValueReturned, error.Message ) );
					}
				}
			} );
	}
}