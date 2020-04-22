using System.Net;
using JetBrains.Annotations;

namespace Tewl.Tools {
	/// <summary>
	/// Tools supporting DNS, URIs, encoding, and so forth.
	/// </summary>
	[ PublicAPI ] 
	public class NetTools {
		/// <summary>
		/// Returns the host name of the local computer.
		/// </summary>
		public static string GetLocalHostName() {
			return Dns.GetHostEntry( "" ).HostName;
		}
	}
}