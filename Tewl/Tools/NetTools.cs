using System.Net;

namespace Tewl.Tools {
	/// <summary>
	/// Tools supporting DNS, URIs, encoding, and so forth.
	/// </summary>
	public class NetTools {
		/// <summary>
		/// Returns the host name of the local computer.
		/// </summary>
		public static string GetLocalHostName() {
			return Dns.GetHostEntry( "" ).HostName;
		}
	}
}