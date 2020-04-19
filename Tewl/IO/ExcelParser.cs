using System;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;

namespace Tewl.IO {
	internal class ExcelParser : Parser {
		public ParsedLine Parse( object line ) {
			var fields = new List<string>();

			return new ParsedLine( fields );
		} 
		
	}
}
