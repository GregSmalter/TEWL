using ClosedXML.Excel;
using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing;

internal class ExcelParser: TabularDataParser {
	private readonly XLWorkbook workbook;

	public ExcelParser( string filePath ) => workbook = new XLWorkbook( filePath );

	public ExcelParser( Stream fileStream ) => workbook = new XLWorkbook( fileStream );

	public override void ParseAndProcessAllLines(
		LineProcessingMethod lineHandler, ICollection<ValidationError> validationErrors, bool disableLineProcessingErrorAccumulation = false ) {
		var ws1 = workbook.Worksheets.First();
		var rows = ws1.RangeUsed().RowsUsed().ToList();
		rows = rows.Where( r => !r.IsEmpty() ).ToList();
		var header = rows.First();
		var headerFields = header.Cells().ToList().Select( c => c.Value.ToString().ToLower() ).ToList();
		foreach( var row in rows.Skip( HeaderRows ) ) {
			ParsedLine parsedLine = new ExcelParsedLine( headerFields, row );
			NonHeaderRows++;
			if( parsedLine.ContainsData ) {
				RowsContainingData++;
				var validator = new Validator();
				lineHandler( validator, parsedLine );
				if( !validator.ErrorsOccurred )
					RowsWithoutValidationErrors++;
				else if( !disableLineProcessingErrorAccumulation )
					foreach( var error in validator.Errors )
						validationErrors.Add( new ValidationError( "Line " + parsedLine.LineNumber, error.UnusableValueReturned, error.Message ) );
			}
		}
	}
}