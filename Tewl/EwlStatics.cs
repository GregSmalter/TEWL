using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Tewl {
	/// <summary>
	/// A collection of miscellaneous statics that may be useful.
	/// </summary>
	public static partial class EwlStatics {
		/// <summary>
		/// GMS NOTE: Sam has a better version of this.
		/// Runs the specified program with the specified arguments and passes in the specified input. Optionally waits for the program to exit, and throws an
		/// exception if this is specified and a nonzero exit code is returned. If the program is in a folder that is included in the Path environment variable,
		/// specify its name only. Otherwise, specify a path to the program. In either case, you do NOT need ".exe" at the end. Specify the empty string for input
		/// if you do not wish to pass any input to the program.
		/// Returns the output of the program if waitForExit is true.  Otherwise, returns the empty string.
		/// </summary>
		/// <param name="program"></param>
		/// <param name="arguments">Do not pass null.</param>
		/// <param name="input">Do not pass null.</param>
		/// <param name="waitForExit"></param>
		/// <param name="workingDirectory">Do not pass null. Pass the empty string for the current working directory.</param>
		public static string RunProgram( string program, string arguments, string input, bool waitForExit, string workingDirectory = "" ) {
			var outputResult = "";
			using( var p = new Process() ) {
				p.StartInfo.FileName = program;
				p.StartInfo.Arguments = arguments;
				p.StartInfo.CreateNoWindow = true; // prevents command window from appearing
				p.StartInfo.UseShellExecute = false; // necessary for redirecting output
				p.StartInfo.WorkingDirectory = workingDirectory;
				p.StartInfo.RedirectStandardInput = true;
				if( waitForExit ) {
					// Set up output recording.
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;
					var output = new StringWriter();
					var errorOutput = new StringWriter();
					p.OutputDataReceived += ( ( sender, e ) => output.WriteLine( e.Data ) );
					p.ErrorDataReceived += ( ( sender, e ) => errorOutput.WriteLine( e.Data ) );

					p.Start();

					// Begin recording output.
					p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					// Pass input to the program.
					if( input.Length > 0 ) {
						p.StandardInput.Write( input );
						p.StandardInput.Flush();
					}

					// Throw an exception after the program exits if the code is not zero. Include all recorded output.
					p.WaitForExit();
					outputResult = output.ToString();
					if( p.ExitCode != 0 )
						using( var sw = new StringWriter() ) {
							sw.WriteLine( "Program exited with a nonzero code." );
							sw.WriteLine();
							sw.WriteLine( "Program: " + program );
							sw.WriteLine( "Arguments: " + arguments );
							sw.WriteLine();
							sw.WriteLine( "Output:" );
							sw.WriteLine( outputResult );
							sw.WriteLine();
							sw.WriteLine( "Error output:" );
							sw.WriteLine( errorOutput.ToString() );
							throw new ApplicationException( sw.ToString() );
						}
				}
				else {
					p.Start();
					if( input.Length > 0 ) {
						p.StandardInput.Write( input );
						p.StandardInput.Flush();
					}
				}
				return outputResult;
			}
		}

		/// <summary>
		/// Returns an Object with the specified Type and whose value is equivalent to the specified object.
		/// </summary>
		/// <param name="value">An Object that implements the IConvertible interface.</param>
		/// <param name="conversionType">The Type to which value is to be converted.</param>
		/// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
		/// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
		/// reference and conversionType is not a value type.</returns>
		/// <remarks>
		/// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
		/// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
		/// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
		/// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
		/// This method was written by Peter Johnson at:
		/// http://aspalliance.com/author.aspx?uId=1026.
		/// </remarks>
		public static object ChangeType( object value, Type conversionType ) {
			// This if block was taken from Convert.ChangeType as is, and is needed here since we're
			// checking properties on conversionType below.
			if( conversionType == null )
				throw new ArgumentNullException( "conversionType" );

			// If it's not a nullable type, just pass through the parameters to Convert.ChangeType

			if( conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) ) {
				// It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
				// InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
				// determine what the underlying type is
				// If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
				// have a type--so just return null
				// We only do this check if we're converting to a nullable type, since doing it outside
				// would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
				// value is null and conversionType is a value type.
				if( value == null )
					return null;

				// It's a nullable type, and not null, so that means it can be converted to its underlying type,
				// so overwrite the passed-in conversion type with this underlying type
				var nullableConverter = new NullableConverter( conversionType );
				conversionType = nullableConverter.UnderlyingType;
			} // end if

			// Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
			// nullable type), pass the call on to Convert.ChangeType
			return Convert.ChangeType( value, conversionType );
		}

		/// <summary>
		/// Recursively calls Path.Combine on the given paths.  Path is returned without a trailing slash.
		/// </summary>
		public static string CombinePaths( string one, string two, params string[] paths ) {
			if( one == null || two == null )
				throw new ArgumentException( "String cannot be null." );

			var pathList = new List<string>( paths );
			pathList.Insert( 0, two );
			pathList.Insert( 0, one );

			var combinedPath = "";

			foreach( var path in pathList )
				combinedPath += getTrimmedPath( path );

			return combinedPath.TrimEnd( '\\' );
		}

		private static string getTrimmedPath( string path ) {
			path = path.Trim( '\\' );
			path = path.Trim();
			if( path.Length > 0 )
				return path + "\\";
			return "";
		}

		/// <summary>
		/// Converts a boolean into a decimal for storage in Oracle.
		/// </summary>
		public static decimal BooleanToDecimal( this bool b ) {
			return b ? 1 : 0;
		}

		/// <summary>
		/// Converts a decimal (presumably from Oracle) into a boolean.
		/// </summary>
		public static bool DecimalToBoolean( this decimal d ) {
			if( d == 1 )
				return true;
			if( d == 0 )
				return false;
			throw new ApplicationException( "Unknown decimal value encountered when converting to boolean." );
		}

		/// <summary>
		/// Returns the first element of the list.  Returns null if the list is empty.
		/// </summary>
		public static T FirstItem<T>( this List<T> list ) where T: class {
			return list.Count == 0 ? null : list[ 0 ];
		}

		/// <summary>
		/// Returns the last element of the list.  Returns null if the list is empty.
		/// </summary>
		public static T LastItem<T>( this List<T> list ) where T: class {
			return list.Count == 0 ? null : list[ list.Count - 1 ];
		}

		/// <summary>
		/// Returns the host name of the local computer.
		/// </summary>
		public static string GetLocalHostName() {
			return Dns.GetHostEntry( "" ).HostName;
		}

		/// <summary>
		/// Creates a collection containing only this item.
		/// </summary>
		public static IReadOnlyCollection<T> ToCollection<T>( this T item ) {
			return ImmutableArray.Create( item );
		}

		/// <summary>
		/// Returns an enumerable of functions that return the given items.
		/// </summary>
		public static IEnumerable<Func<T>> ToFunctions<T>( this IEnumerable<T> items ) {
			return items.Select<T, Func<T>>( i => () => i );
		}

		/// <summary>
		/// Retries the given action until it executes without exception or maxAttempts is reached. You can specify different maxAttempts or retry intervals - the default is 30 tries
		/// with a 2 second wait in between each try.
		/// If every attempt fails, a new application exception with bill thrown with the given message. The original exception will be the inner exception.
		/// </summary>
		public static void Retry( Action action, string failureMessage, int maxAttempts = 30, int retryIntervalMs = 2000 ) {
			for( var i = 0;; i += 1 ) {
				try {
					action();
					break;
				}
				catch( Exception e ) {
					if( i < maxAttempts )
						Thread.Sleep( retryIntervalMs );
					else
						throw new ApplicationException( failureMessage, e );
				}
			}
		}

		/// <summary>
		/// Returns true if the specified objects are equal according to the default equality comparer.
		/// </summary>
		public static bool AreEqual<T>( T x, T y, IEqualityComparer<T> comparer = null ) {
			return ( comparer ?? EqualityComparer<T>.Default ).Equals( x, y );
		}

		/// <summary>
		/// Returns an integer indicating whether the first specified object precedes (negative value), follows (positive value), or occurs in the same position in
		/// the sort order (zero) as the second specified object, according to the default sort-order comparer. If you are comparing strings, Microsoft recommends
		/// that you use a StringComparer instead of the default comparer.
		/// </summary>
		public static int Compare<T>( T x, T y, IComparer<T> comparer = null ) {
			return ( comparer ?? Comparer<T>.Default ).Compare( x, y );
		}

		/// <summary>
		/// Returns the default value of the specified type.
		/// </summary>
		public static T GetDefaultValue<T>( bool useEmptyAsStringDefault ) {
			return typeof( T ) == typeof( string ) && useEmptyAsStringDefault ? (T)(object)"" : default( T );
		}

		/// <summary>
		/// Executes the given block of code as a critical region synchronized on the given GUID. The GUID should be passed with surrounding {}.
		/// The GUID is automatically prefixed with Global\ so that the mutex has machine scope. The GUID will usually be one to one with a program.
		/// Pass true for SkipExecutionIfMutexAlreadyOwned to return if something already has the mutex.  This is useful for killing a program when
		/// you only want one instance to run at a time. Pass false if you want to wait until the mutex is released to run your code.
		/// Returns false if execution was skipped.  Otherwise, returns true.
		/// If using this along with a WithStandardExceptionHandling method, this should go inside.
		/// </summary>
		public static bool ExecuteAsCriticalRegion( string guid, bool skipExecutionIfMutexAlreadyOwned, Action method ) {
			// The Global\ prefix makes the mutex visible across terminal services sessions. The double backslash is convention.
			// NOTE: What double backslash? Isn't it a single backslash as the comment states?
			guid = "Global\\" + guid;

			using( var mutex = new Mutex( false /*Do not try to immediately acquire the mutex*/, guid ) ) {
				if( skipExecutionIfMutexAlreadyOwned )
					try {
						if( !mutex.WaitOne( 0 ) )
							return false;
					}
					catch( AbandonedMutexException ) {}

				try {
					// AbandonedMutexException exists to warn us that data might be corrupt because another thread didn't properly release the mutex. We ignore it because
					// in our case, we only use the mutex in one thread per process (NOTE: This is true, but only by coincidence) and therefore don't need to worry about data corruption.
					// AbandonedMutexExceptions are thrown when the mutex is acquired, not when it is abandoned. Therefore, only the one thread that acquires the mutex
					// next will have to deal with the exception. For this reason, we are OK here in terms of only letting one thread execute its method at a time.
					try {
						// Acquire the mutex, waiting if necessary.
						mutex.WaitOne();
					}
					catch( AbandonedMutexException ) {}

					method();
				}
				finally {
					// We release the mutex manually since, yet again, nobody can agree on whether the Dispose() method called at the end of the using block always properly
					// does this for us.  Some have reported you need to do what we are doing here, so for safety's sake, we have our own finally block.

					mutex.ReleaseMutex();
				}
			}
			return true;
		}

		/// <summary>
		/// Transforms the underlying value of this nullable object using the specified selector, if an underlying value exists.
		/// </summary>
		public static DestinationType? ToNewUnderlyingValue<SourceType, DestinationType>( this SourceType? value, Func<SourceType, DestinationType> valueSelector )
			where SourceType: struct where DestinationType: struct =>
			value.HasValue ? (DestinationType?)valueSelector( value.Value ) : null;
	}
}