using System.Collections.Generic;
using ClosedXML.Excel;

namespace Tewl.IO.TabularDataParsing {
	class ExcelParsedLine: ParsedLine {
		private readonly List<string> headerFields;
		private readonly IXLRangeRow row;

		public ExcelParsedLine( List<string> headerFields, IXLRangeRow row ) {
			this.headerFields = headerFields;
			this.row = row;
		}

		public bool ContainsData => !row.IsEmpty();

		public int LineNumber => row.RowNumber();

		// Index is zero-based. The Cell() function on the IXLRangeRow is 1-based. 
		public string this[ int index ] => row.Cell( index + 1 ).Value.ToString();

		// Index is zero-based. The Cell() function on the IXLRangeRow is 1-based. 
		public string this[ string columnName ] => row.Cell( headerFields.IndexOf( columnName.ToLower() ) + 1 ).Value.ToString();

		public bool ContainsField( string fieldName ) => headerFields.Contains( fieldName );
	}
}