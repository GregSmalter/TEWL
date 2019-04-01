using System;
using System.Text;

namespace Tewl.Exceptions {
	/// <inheritdoc />
	/// <summary>
	/// Thrown when there is an error running a program.
	/// </summary>
	public class RunProgramException: Exception {
		public RunProgramException( int? errorCode, string message, string processOutput, string processError, Exception innerException = null ): base(
			getMessage( errorCode, message, processOutput, processError ),
			innerException ) { }

		private static string getMessage( int? errorCode, string message, string processOutput, string processError ) {
			try {
				var sb = new StringBuilder();
				sb.AppendLine( message );
				if( errorCode != null )
					sb.AppendLine( "Error code: " + errorCode );
				if( processOutput != null )
					sb.AppendLine( "Process Output: " + processOutput );
				if( processError != null )
					sb.AppendLine( "Process Error Output: " + processError );

				return sb.ToString();
			}
			catch {
				// Could fail with OutOfMemoryException
				return "";
			}
		}
	}
}