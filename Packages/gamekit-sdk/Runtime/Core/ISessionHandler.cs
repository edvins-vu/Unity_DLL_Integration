using System;
using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;
using Nakama;

namespace Estoty.Gamekit.Core
{
	public interface ISessionHandler: IDisposable
	{
		Task<Response> Logout();
		Task<Response> RefreshAccount();
		Task<IResponse> RestoreSession();
		Task AttemptRestoreSession();
		bool Initialized { get; }
		ISession Session { get; }
		IApiAccount Account { get; }
		event Action OnSessionChange;
		event Action OnInitialized;
	}
}