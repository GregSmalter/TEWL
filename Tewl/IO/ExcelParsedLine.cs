using System.Collections.Generic;
using ClosedXML.Excel;

namespace Tewl.IO {
	class ExcelParsedLine: TabularDataParsedLine {
		private readonly List<string> headerFields;
		private readonly IXLRangeRow row;

		public ExcelParsedLine( List<string> headerFields, IXLRangeRow row ) {
			this.headerFields = headerFields;
			this.row = row;
		}

		public bool ContainsData => !row.IsEmpty();

		public int LineNumber => row.RowNumber();

		public string this[ int index ] => row.Cell( index ).Value.ToString();

		public string this[ string columnName ] => row.Cell( headerFields.IndexOf( columnName ) ).Value.ToString();

		public bool ContainsField( string fieldName ) => headerFields.Contains( fieldName );
	}
}