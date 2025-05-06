using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;

namespace Estoty.GameKit.Authentication.Providers
{
	public interface IAuthProvider
	{
		bool Supported { get; }
		bool Authenticated { get; }
		Task<Response> Authenticate();
	}
}