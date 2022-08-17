using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Tewl.Exceptions;

namespace Tewl.Tools {
	/// <summary>
	/// Convenience methods for invoking external Processes.
	/// </summary>
	[ PublicAPI ]
	public static class ProcessTools {
		/// <summary>
		/// Runs the specified program with the specified arguments and passes in the specified input. If the program is in a
		/// folder that is included in the Path environment variable,
		/// specify its name only. Otherwise, specify a path to the program. In either case, you do not need ".exe" at the end.
		/// Specify the empty string for input
		/// if you do not wish to pass any input to the program.
		/// Returns the output of the program.
		/// </summary>
		/// <param name="program">A command or exe.</param>
		/// <param name="arguments">Do not pass null.</param>
		/// <param name="input">Do not pass null.</param>
		/// <param name="workingDirectory">Do not pass null. Pass the empty string for the current working directory.</param>
		/// <param name="waitForExitTimeout">
		/// Waits for given duration for the program exists before throwing an exception. Null
		/// will wait for exit indefinitely.!
		/// </param>
		/// <exception cref="RunProgramException">When any error occurs.</exception>
		/// <returns>The output of the program. Does not include standard error.</returns>
		public static string RunProgram( string program, string arguments, string input = "", string workingDirectory = "", TimeSpan? waitForExitTimeout = null ) {
			var sbOutput = new StringBuilder();
			var sbError = new StringBuilder();
			try {
				RunProgram(
					program,
					arguments,
					msg => sbOutput.AppendLine( msg ),
					msg => sbError.AppendLine( msg ),
					input,
					workingDirectory,
					waitForExitTimeout );
			}
			catch( RunProgramException e ) {
				e.ProcessOutput = sbOutput.ToString();
				e.ProcessError = sbError.ToString();
				throw;
			}

			return sbOutput.ToString();
		}

		/// <summary>
		/// Runs the specified program with the specified arguments and passes in the specified input. If the program is in a
		/// folder that is included in the Path environment variable,
		/// specify its name only. Otherwise, specify a path to the program. In either case, you do not need ".exe" at the end.
		/// Specify the empty string for input
		/// if you do not wish to pass any input to the program.
		/// </summary>
		/// <param name="program">A command or exe.</param>
		/// <param name="arguments">Do not pass null.</param>
		/// <param name="outputWriter">A function accepting a line of output.</param>
		/// <param name="errorOutputWriter">A function accepting a line of error output.</param>
		/// <param name="input">Do not pass null.</param>
		/// <param name="workingDirectory">Do not pass null. Pass the empty string for the current working directory.</param>
		/// <param name="waitForExitTimeout">
		/// Waits for given duration for the program exists before throwing an exception. Null
		/// will wait for exit indefinitely.!
		/// </param>
		/// <exception cref="RunProgramException">When any error occurs.</exception>
		public static void RunProgram(
			string program, string arguments, Action<string> outputWriter, Action<string> errorOutputWriter, string input = "", string workingDirectory = "",
			TimeSpan? waitForExitTimeout = null ) {
			try {
				var p = new Process();
				p.StartInfo.FileName = program;
				p.StartInfo.Arguments = arguments;
				p.StartInfo.CreateNoWindow = true; // prevents command window from appearing
				p.StartInfo.UseShellExecute = false; // necessary for redirecting output
				p.StartInfo.WorkingDirectory = workingDirectory;

				p.StartInfo.RedirectStandardInput = true;

				// Set up output recording.
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;

				p.OutputDataReceived += ( o, args ) => getOutputHandler( args, outputWriter );
				p.ErrorDataReceived += ( o, args ) => getOutputHandler( args, errorOutputWriter );

				if( !p.Start() )
					throw new ApplicationException( "Process failed to start." );

				int? processExitCode = null;
				try {
					// Begin recording output.
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					// Pass input to the program.
					if( input.Length > 0 ) {
						p.StandardInput.Write( input );
						p.StandardInput.Flush();
					}

					if( waitForExitTimeout != null && !p.WaitForExit( (int)waitForExitTimeout.Value.TotalMilliseconds ) )
						throw new ApplicationException( $"Process did not exit within timeout '{waitForExitTimeout}'." );

					/* When standard output has been redirected to asynchronous event handlers, it is possible that output processing will not
					 * have completed when this method returns. To ensure that asynchronous event handling has been completed, call the WaitForExit()
					 * overload that takes no parameter after receiving a true from this overload.
					 * https://msdn.microsoft.com/en-us/library/ty0d8k56(v=vs.110)
				 */
					// This will wait infinitely.
					p.WaitForExit();

					processExitCode = p.ExitCode;

					// Throw an exception after the program exits if the code is not zero. Include all recorded output.
					if( processExitCode != 0 )
						throw new ApplicationException( $"Process exited with non-zero code '{processExitCode}'." );
				}
				catch( Exception ex ) {
					throw new RunProgramException( $"An exception was thrown. {nameof(program)}: {program}. {nameof(arguments)}: {arguments}", processExitCode, ex );
				}
				finally {
					ensureProcessExited( p );
				}
			}
			catch( Exception ex ) when( !( ex is RunProgramException ) ) {
				throw new RunProgramException( $"An exception was thrown. {nameof(program)}: {program}. {nameof(arguments)}: {arguments}", null, ex );
			}
		}

		/// <summary>
		/// Appends the output as long as the data is not null.
		/// </summary>
		private static void getOutputHandler( DataReceivedEventArgs args, Action<string> output ) {
			if( args.Data == null )
				return;
			output( args.Data );
		}

		private static void ensureProcessExited( Process process ) {
			if( !process?.HasExited ?? false ) {
				try {
					process.Kill();
				}
				catch { }
			}

			process?.Dispose();
		}
	}
}