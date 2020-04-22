using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tewl.Tools {
	public static class TimingTools {
		/// <summary>
		/// Times the execution of the given function. Returns the duration.
		/// </summary>
		public static TimeSpan TimeThis( [ InstantHandle ] Action func ) {
			var watch = Stopwatch.StartNew();
			func();
			return watch.Elapsed;
		}

		/// <summary>
		/// Times the execution of the given function. Returns the duration.
		/// </summary>
		public static async Task<TimeSpan> TimeThis( [ InstantHandle ] Func<Task> func ) {
			var watch = Stopwatch.StartNew();
			await func();
			return watch.Elapsed;
		}

		/// <summary>
		/// Times the execution of the given function. Returns the duration and your return value.
		/// </summary>
		public static (TimeSpan Duration, T Result) TimeThis<T>( [ InstantHandle ] Func<T> a ) {
			var stop = Stopwatch.StartNew();
			var r = a();
			stop.Stop();
			return ( stop.Elapsed, r );
		}

		/// <summary>
		/// Times the execution of the given function. Returns the duration and your return value.
		/// </summary>
		public static async Task<(TimeSpan Duration, T Result)> TimeThis<T>( [ InstantHandle ] Func<Task<T>> a ) {
			var stop = Stopwatch.StartNew();
			var r = await a();
			stop.Stop();
			return ( stop.Elapsed, r );
		}
	}
}