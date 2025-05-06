using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;

namespace Estoty.Gamekit.Core
{
	public interface IRpcHandler
	{
		Task<Response<T>> SendRequest<T>(string endpoint);
		Task<Response> SendRequest(string endpoint, object payload);
		Task<Response<T>> SendRequest<T>(string endpoint, object payload);
	}
}