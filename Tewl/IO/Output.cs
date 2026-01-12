namespace Tewl.IO;

/// <summary>
/// Helps communicate with standard out, standard error, and files.
/// </summary>
[ PublicAPI ]
public class Output {
	/// <summary>
	/// Permanently redirects standard output and error to file, with autoflushing enabled.
	/// </summary>
	public static void RedirectOutputToFile( string outputFileName, string errorFileName ) {
		var outputWriter = new StreamWriter( outputFileName, true );
		var errorWriter = new StreamWriter( errorFileName, true );
		outputWriter.AutoFlush = true;
		errorWriter.AutoFlush = true;
		Console.SetOut( outputWriter );
		Console.SetError( errorWriter );
	}
}