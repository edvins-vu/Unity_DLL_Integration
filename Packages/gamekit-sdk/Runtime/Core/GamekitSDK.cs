using System.Threading;
using System.Threading.Tasks;
using Estoty.Gamekit.Core;
using Estoty.GameKit.Utility.Responses;
using Nakama;

namespace Estoty.Gamekit
{
	public class GamekitSDK
	{
		private readonly RpcHandler _rpcHandler;

		//public GamekitSDK(string url, string port, string apiKey)
		//{
		//	IClient client = new Client("defaultkey", url, ushort.Parse(port), true);			
		//	CancellationToken cancellationToken = new CancellationTokenSource().Token;
		//	ILogger logger = new ConsoleLogger(); // Adjust logging setup

		//	ISessionHandler sessionHandler = new SessionHandler(cancellationToken, authHandler, client, logger); // Ensure session management

		//	_rpcHandler = new RpcHandler(logger, client, sessionHandler, cancellationToken);
		//}

		public async Task<Response<string>> GetMail(string userId)
		{
			return await _rpcHandler.SendRequest<string>($"get_mail/{userId}");
		}
	}
}