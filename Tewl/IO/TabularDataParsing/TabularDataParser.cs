using Tewl.InputValidation;

namespace Tewl.IO.TabularDataParsing;

/// <summary>
/// Use this to process several lines of any type of tabular data, such as CSVs, fixed-width data files, or Excel files.
/// </summary>
[ PublicAPI ]
public abstract class TabularDataParser {
	/// <summary>
	/// Method that knows how to process a line from a particular file.  The validator is new for each row and has no errors,
	/// initially.
	/// </summary>
	public delegate void LineProcessingMethod( ParsedLine line, Validator validator );

	/// <summary>
	/// Header rows to skip, shared by all parsers.
	/// </summary>
	protected int headerRowsToSkip;

	/// <summary>
	/// Has a value if there is a header row. Also implies header rows to skip is 1.
	/// </summary>
	protected IReadOnlyCollection<string>? requiredColumns;

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
	public int HeaderRows => requiredColumns is not null ? 1 : headerRowsToSkip;

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
	protected TabularDataParser() {}

	/// <summary>
	/// Creates a parser designed to parse a file with fixed data column widths. Specify the starting position of each column
	/// (using one-based column index).
	/// Characters that take up more than 1 unit of width, such as tabs, can cause problems here.
	/// </summary>
	public static TabularDataParser CreateForFixedWidthFile( string filePath, int headerRowsToSkip, params int[] columnStartPositions ) =>
		FixedWidthParser.CreateWithFilePath( filePath, headerRowsToSkip, columnStartPositions );

	/// <summary>
	/// Creates a parser designed to parse a CSV file.
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="requiredColumns">If the file has a header row, you must pass a collection (empty or not) for this parameter, which will allow you to access
	/// fields using the column name in addition to the index. If the collection is nonempty and any of the specified columns are missing, ParseAndProcessAllLines
	/// will generate validation errors and return without processing any lines. Pass null for this parameter if the file does not have a header row.</param>
	public static TabularDataParser CreateForCsvFile( string filePath, IReadOnlyCollection<string>? requiredColumns ) =>
		new CsvLineParser( filePath ) { requiredColumns = requiredColumns };

	/// <summary>
	/// Creates a parser designed to parse a CSV file.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="requiredColumns">If the file has a header row, you must pass a collection (empty or not) for this parameter, which will allow you to access
	/// fields using the column name in addition to the index. If the collection is nonempty and any of the specified columns are missing, ParseAndProcessAllLines
	/// will generate validation errors and return without processing any lines. Pass null for this parameter if the file does not have a header row.</param>
	public static TabularDataParser CreateForCsvFile( Stream stream, IReadOnlyCollection<string>? requiredColumns ) =>
		new CsvLineParser( stream ) { requiredColumns = requiredColumns };

	/// <summary>
	/// Assumes header row. Fields will always be accessible by name.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="requiredColumns">If any of the columns specified in this collection are missing, ParseAndProcessAllLines will generate validation errors and
	/// return without processing any lines.</param>
	public static TabularDataParser CreateForExcelFile( Stream stream, IReadOnlyCollection<string> requiredColumns ) =>
		new ExcelParser( stream ) { requiredColumns = requiredColumns };

	/// <summary>
	/// Assumes header row. Fields will always be accessible by name.
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="requiredColumns">If any of the columns specified in this collection are missing, ParseAndProcessAllLines will generate validation errors and
	/// return without processing any lines.</param>
	public static TabularDataParser CreateForExcelFile( string filePath, IReadOnlyCollection<string> requiredColumns ) =>
		new ExcelParser( filePath ) { requiredColumns = requiredColumns };

	/// <summary>
	/// For every line (after headerRowsToSkip) in the file with the given path, calls the line handling method you pass.
	/// The validationErrors collection will hold all validation errors encountered during the processing of all lines.
	/// Each line handler method will be given a fresh validator to do its work with.
	/// </summary>
	/// <param name="lineHandler"></param>
	/// <param name="validationErrors"></param>
	/// <param name="disableLineProcessingErrorAccumulation">Pass true to only use the error collection for missing columns. This is useful when processing
	/// extremely large data sets, since accumulating all line-processing errors in one collection may result in high memory usage.</param>
	public abstract void ParseAndProcessAllLines(
		LineProcessingMethod lineHandler, ICollection<DataValidationError> validationErrors, bool disableLineProcessingErrorAccumulation = false );
}