using System;
using System.Text;
using JetBrains.Annotations;

namespace Tewl.Exceptions {
	/// <inheritdoc />
	/// <summary>
	/// Thrown when there is an error running a program.
	/// </summary>
	[ PublicAPI ]
	public class RunProgramException: Exception {
		private readonly string message;
		private readonly int? errorCode;
		internal string ProcessOutput;
		internal string ProcessError;

		public RunProgramException( string message, int? errorCode, Exception innerException = null ):base(null, innerException) {
			this.message = message;
			this.errorCode = errorCode;
		}

		public override string Message {
			get {
				try {
					var sb = new StringBuilder();
					sb.AppendLine( message );
					if( errorCode != null )
						sb.AppendLine( "Error code: " + errorCode );
					if( ProcessOutput != null )
						sb.AppendLine( "Process Output: " + ProcessOutput );
					if( ProcessError != null )
						sb.AppendLine( "Process Error Output: " + ProcessError );

					return sb.ToString();
				}
				catch {
					// Could fail with OutOfMemoryException
					return "";
				}
			}
		}
	}
}