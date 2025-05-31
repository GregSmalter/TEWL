using ClosedXML.Excel;
using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing;

internal class ExcelParser: TabularDataParser {
	private readonly XLWorkbook workbook;

	public ExcelParser( string filePath ) => workbook = new XLWorkbook( filePath );

	public ExcelParser( Stream fileStream ) => workbook = new XLWorkbook( fileStream );

	public override void ParseAndProcessAllLines(
		LineProcessingMethod lineHandler, ICollection<DataValidationError> validationErrors, bool disableLineProcessingErrorAccumulation = false ) {
		var ws1 = workbook.Worksheets.First();
		var rows = ws1.RangeUsed().RowsUsed().ToList();
		rows = rows.Where( r => !r.IsEmpty() ).ToList();
		var header = rows.First();

		var columnIndicesByName = header.Cells().Select( ( cell, index ) => ( cell.Value.ToString(), index ) ).ToDictionary( StringComparer.OrdinalIgnoreCase );

		var missingColumns = requiredColumns!.Where( i => !columnIndicesByName.ContainsKey( i ) ).Materialize();
		if( missingColumns.Any() ) {
			var columnList = StringTools.GetEnglishListPhrase( missingColumns.Select( i => $"“{i}”" ), true );
			var singularize = missingColumns.Count == 1;
			validationErrors.Add(
				new DataValidationError(
					"Header row",
					false,
					$"The required {( singularize ? "column" : "columns" )} {columnList} {( singularize ? "is" : "are" )} missing." ) );
			return;
		}

		foreach( var row in rows.Skip( HeaderRows ) ) {
			ParsedLine parsedLine = new ExcelParsedLine( columnIndicesByName, row );
			NonHeaderRows++;
			if( parsedLine.ContainsData ) {
				RowsContainingData++;
				var validator = new Validator();
				lineHandler( parsedLine, validator );
				if( !validator.ErrorsOccurred )
					RowsWithoutValidationErrors++;
				else if( !disableLineProcessingErrorAccumulation )
					foreach( var error in validator.Errors )
						validationErrors.Add( new DataValidationError( "Row " + parsedLine.LineNumber, error.UnusableValueReturned, error.Message ) );
			}
		}
	}
}