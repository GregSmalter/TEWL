using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing {
	internal class ExcelParser: TabularDataParser {
		readonly XLWorkbook workbook;

		public ExcelParser( string filePath ) => workbook = new XLWorkbook( filePath );

		public ExcelParser( Stream fileStream ) => workbook = new XLWorkbook( fileStream );

		/// <summary>
		/// For every line (after headerRowsToSkip) in the file with the given path, calls the line handling method you pass.
		/// The validationErrors collection will hold all validation errors encountered during the processing of all lines.
		/// When processing extremely large data sets, accumulating validationErrors in one collection may result in high memory
		/// usage. To avoid
		/// this, use the overload without this collection.
		/// Each line handler method will be given a fresh validator to do its work with.
		/// </summary>
		public override void ParseAndProcessAllLines( LineProcessingMethod lineHandler, ICollection<ValidationError> validationErrors ) {
			var ws1 = workbook.Worksheets.First();
			var rows = ws1.RangeUsed().RowsUsed().ToList();
			rows = rows.Where( r => !r.IsEmpty() ).ToList();
			var header = rows.First();
			var headerFields = header.Cells().ToList().Select( c => c.Value.ToString().ToLower() ).ToList();
			foreach( var row in rows.Skip( HeaderRows ) ) {
				var parsedLine = new ExcelParsedLine( headerFields, row );
				NonHeaderRows++;
				if( parsedLine.ContainsData ) {
					RowsContainingData++;
					var validator = new Validator();
					lineHandler( validator, parsedLine );
					if( validator.ErrorsOccurred ) {
						if( validationErrors != null ) {
							foreach( var error in validator.Errors )
								validationErrors.Add( new ValidationError( "Line " + parsedLine.LineNumber, error.UnusableValueReturned, error.Message ) );
						}
					}
					else
						RowsWithoutValidationErrors++;
				}
			}
		}
	}
}