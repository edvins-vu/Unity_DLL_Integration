using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Estoty.GameKit.Authentication;
using Estoty.GameKit.Authentication.Providers;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using UnityEngine;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace Estoty.Gamekit.Core
{
	public class GamekitSDK : IDisposable
	{
		private readonly IClient _client;
		private readonly ISessionHandler _sessionHandler;
		private readonly IRpcHandler _rpcHandler;
		private readonly ILogger _logger;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly CancellationToken _cancellationToken;

		public SessionHandler SessionHandler
		{
			get { return (SessionHandler)_sessionHandler; }
		}

		// Setting up the GamekitSDK with URL, Port, and API Key
		public GamekitSDK(string url, string port, string apiKey)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			// Initialize a default logger
			_logger = new DefaultLogger();

			var serverConfig = ServerConfigRecord.Instance;

			serverConfig.Protocol = Uri.UriSchemeHttp.ToString();
			serverConfig.Host = url;
			serverConfig.Port = int.Parse(port);
			serverConfig.Key = apiKey;

			_client = new Client(serverConfig.Protocol, serverConfig.Host, serverConfig.Port, serverConfig.Key);

			// Initialize the authentication provider
			IAuthProvider authProvider = new DefaultAuthProvider();

			var authHandler = new AuthHandler(Application.version, _client, _logger, authProvider);
			_sessionHandler = new SessionHandler(_cancellationTokenSource, authHandler, _client, _logger);
			_rpcHandler = new RpcHandler(_logger, _client, _sessionHandler, _cancellationToken);

			// Session restoration process
			_sessionHandler.AttemptRestoreSession();
			_logger.Info($"[{nameof(GamekitSDK)}] GamekitSDK initialized with URL: {url}, Port: {port}, API Key: {apiKey}");
		}

		public async Task<Response<MailboxResponse>> GetMail(string userId)
		{
			const string rpcId = "gamekit_server_mailbox_list_rpc";
			Dictionary<string, string> parameters = new Dictionary<string, string> { { "user_id", userId } };

			try
			{
				return await _rpcHandler.SendRequest<MailboxResponse>(rpcId, parameters);
			}
			catch (Exception e)
			{
				_logger.Error($"[GamekitSDK] Error during GetMail RPC for user {userId}: {e}");
				return new Response<MailboxResponse>(e);
			}
		}

		public async Task<Response<MessageResponse>> ReadNotification(string notificationId)
		{
			const string rpcId = "gamekit_client_mailbox_read_rpc";

			Dictionary<string, object> parameters = new Dictionary<string, object>
			{
				{ "ids", new string[] { notificationId } }
			};

			try
			{
				return await _rpcHandler.SendRequest<MessageResponse>(rpcId, parameters);
			}
			catch (Exception e)
			{
				_logger.Error($"[GamekitSDK] Error during ReadNotification RPC for user {notificationId}: {e}");
				return new Response<MessageResponse>(e);
			}
		}

		public void Dispose()
		{
			_sessionHandler?.Dispose();
			_cancellationTokenSource?.Dispose();
		}
	}
}