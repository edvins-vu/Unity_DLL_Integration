using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;

namespace Estoty.GameKit.Authentication.Providers
{
	public class MockAuthProvider : IAuthProvider
	{
		public bool Authenticated { get; private set; }
		public bool Supported => true;

		public async Task<Response> Authenticate()
		{
			Authenticated = true;
			return new Response();
		}
	}
}