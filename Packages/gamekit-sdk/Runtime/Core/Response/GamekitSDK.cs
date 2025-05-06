using System;
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
    public partial class GamekitSDK : IDisposable
    {
        private readonly IClient _client;
        private readonly ISessionHandler _sessionHandler;
        private readonly IRpcHandler _rpcHandler;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        public GamekitSDK(string url, string port, string apiKey, string appVersion)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            // Initialize a default logger
            _logger = new DefaultLogger();

            var serverConfig = ScriptableObject.CreateInstance<ServerConfigRecord>();

            _client = new Client(serverConfig.Protocol, serverConfig.Host, serverConfig.Port, serverConfig.Key);

            // For now initialize a default, to be replaced with specific one then
            IAuthProvider authProvider = new DefaultAuthProvider();

            var authHandler = new AuthHandler(appVersion, _client, _logger, authProvider);
            _sessionHandler = new SessionHandler(_cancellationTokenSource, authHandler, _client, _logger);
            _rpcHandler = new RpcHandler(_logger, _client, _sessionHandler, _cancellationToken);

            // Start the session restoration process
            _sessionHandler.AttemptRestoreSession();
        }

        // Example method to get mail
        public async Task<Response<MailboxResponse>> GetMail(string userId)
        {
            string endpoint = $"api/mail/{userId}";
            var response = await _rpcHandler.SendRequest<MailboxResponse>(endpoint);

            if (response.Failed)
            {
                _logger.Error($"[GamekitSDK] Failed to get mail for user {userId}: {response.Exception}");
            }
            else
            {
                _logger.DebugFormat($"[GamekitSDK] Successfully retrieved mail for user {userId}.");
            }

            return response; // Return the Response object containing success or failure information
        }

        public void Dispose()
        {
            _sessionHandler?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}