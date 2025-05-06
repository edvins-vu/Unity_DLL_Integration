using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;
using Nakama;

namespace Estoty.GameKit.Authentication
{
	public interface IAuthHandler
	{
		Task<Response<ISession>> Restore(string deviceId);
		Task<Response> LogoutAsync(ISession session);
		Task<Response<ISession>> LoginAsync();
	}
}