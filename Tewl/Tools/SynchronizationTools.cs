using System.Threading;
using NodaTime;

namespace Tewl.Tools;

/// <summary>
/// Static methods pertaining to thread and process synchronization.
/// </summary>
[ PublicAPI ]
public static class SynchronizationTools {
	/// <summary>
	/// Executes the specified method as an operating-system-wide critical region synchronized on the given mutex name. Returns false if execution was skipped.
	/// Otherwise, returns true.
	/// </summary>
	/// <param name="mutexName"></param>
	/// <param name="timeout">Pass <see cref="Duration.Zero"/> to return immediately if something already has the mutex. This is useful for killing a program when
	/// you only want one instance to run at a time. Pass null if you want to wait until the mutex is released to run your code.</param>
	/// <param name="method">Takes whether the last thread/process to acquire the mutex abandoned it, which could mean that the resource(s) protected by the mutex
	/// are not in a consistent state.</param>
	// See https://stackoverflow.com/a/229567/35349
	public static bool ExecuteWithMachineExclusiveAccess( string mutexName, Duration? timeout, Action<bool> method ) {
		using var mutex = new Mutex( false, @"Global\" + mutexName );
		var acquired = false;

		try {
			var lastCallInterrupted = false;
			try {
				acquired = mutex.WaitOne( timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : -1 );
			}
			catch( AbandonedMutexException ) {
				acquired = true;
				lastCallInterrupted = true;
			}

			if( acquired )
				method( lastCallInterrupted );
		}
		finally {
			if( acquired )
				mutex.ReleaseMutex();
		}

		return acquired;
	}
}