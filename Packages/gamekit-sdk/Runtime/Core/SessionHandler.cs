using System;
using System.Threading;
using System.Threading.Tasks;
using Estoty.GameKit.Authentication;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using UnityEngine;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace Estoty.Gamekit.Core
{
    public class SessionHandler : ISessionHandler, IDisposable
    {
        public bool Initialized { get; private set; }
        public ISession Session { get; private set; }
        public IApiAccount Account { get; private set; }

        public bool SessionValid => Session != null && !Session.IsExpired;

        private string DeviceId
        {
            get
            {
                string deviceId = PlayerPrefs.GetString(AuthConstants.PLAYER_PREFS_DEVICE_ID);

                if (string.IsNullOrEmpty(deviceId) == false)
                    return deviceId;

                deviceId = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString(AuthConstants.PLAYER_PREFS_DEVICE_ID, deviceId);
                _logger.DebugFormat($"[{nameof(SessionHandler)}] Device ID: {deviceId}"); // Added log here
                return deviceId;
            }
        }

        public event Action OnSessionChange;
        public event Action OnInitialized;

        private CancellationToken CancellationToken => _cancellationTokenSource.Token;

        private readonly TimeSpan _retryCooldown = TimeSpan.FromSeconds(10);

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IAuthHandler _authHandler;
        private readonly IClient _client;
        private readonly ILogger _logger;

        public SessionHandler(
            CancellationTokenSource cancellationTokenSource,
            IAuthHandler authHandler,
            IClient client,
            ILogger logger
        )
        {
            _cancellationTokenSource = cancellationTokenSource;
            _authHandler = authHandler;
            _client = client;
            _logger = logger;
        }

        public async Task<Response> Logout()
        {
            _logger.DebugFormat($"[{nameof(SessionHandler)}] Logout started.");
            Response response = await _authHandler.LogoutAsync(Session);
            _logger.DebugFormat($"[{nameof(SessionHandler)}] Logout response - Failed: {response.Failed}");

            if (response.Failed == false)
            {
                Session = null;
                _logger.DebugFormat($"[{nameof(SessionHandler)}] Session set to null after logout.");
            }

            return response;
        }

        public async Task<Response> RefreshAccount()
        {
            _logger.DebugFormat($"[{nameof(SessionHandler)}] RefreshAccount started.");
            try
            {
                Account = await _client.GetAccountAsync(Session, canceller: CancellationToken);
                _logger.DebugFormat($"[{nameof(SessionHandler)}] RefreshAccount successful.");
                return new Response();
            }
            catch (Exception exception)
            {
                _logger.Error($"[{nameof(SessionHandler)}] Failed to retrieve account: {exception}");
                return new Response(exception);
            }
        }

        public async Task<IResponse> RestoreSession()
        {
            _logger.DebugFormat($"[{nameof(SessionHandler)}] RestoreSession started. Device ID: {DeviceId}"); // Log Device ID
            Response<ISession> response = await _authHandler.Restore(DeviceId);
            _logger.DebugFormat($"[{nameof(SessionHandler)}] _authHandler.Restore response - Failed: {response.Failed}");

            if (response.Failed)
            {
                _logger.Error($"[{nameof(SessionHandler)}] Failed to restore session: {response.Exception}");
                return new Response(response.Exception);
            }

            Session = response.Payload;
            _logger.DebugFormat($"[{nameof(SessionHandler)}] Session restored: {Session != null}, IsExpired: {Session?.IsExpired}");

            Response responseAccount = await RefreshAccount();
            _logger.DebugFormat($"[{nameof(SessionHandler)}] RefreshAccount response - Failed: {responseAccount.Failed}");
            if (responseAccount.Failed)
            {
                Session = null;
                _logger.DebugFormat($"[{nameof(SessionHandler)}] Session set to null after RefreshAccount failure.");
                return new Response(response.Exception);
            }

            OnSessionChange?.Invoke();
            _logger.DebugFormat($"[{nameof(SessionHandler)}] RestoreSession completed successfully.");
            return new Response();
        }

        public async Task AttemptRestoreSession()
        {
            try
            {
                _logger.DebugFormat($"[{nameof(SessionHandler)}] AttemptRestoreSession started.");
                IResponse response = await RestoreSession();

                if (Initialized == false)
                {
                    Initialized = true;
                    OnInitialized?.Invoke();
                    _logger.DebugFormat($"[{nameof(SessionHandler)}] Initialized and OnInitialized invoked.");
                }

                if (response.Failed == false)
                {
                    _logger.DebugFormat($"[{nameof(SessionHandler)}] AttemptRestoreSession successful, returning.");
                    return;
                }
                _logger.DebugFormat($"[{nameof(SessionHandler)}] AttemptRestoreSession failed, delaying and retrying.");
                await Task.Delay(_retryCooldown, CancellationToken);
                await AttemptRestoreSession();
            }
            catch (TaskCanceledException)
            {
                _logger.Error($"[{nameof(SessionHandler)}] Restore session operation canceled.");
            }
        }

        public void Dispose()
        {
            _logger.DebugFormat($"[{nameof(SessionHandler)}] Dispose called.");
            if (CancellationToken.IsCancellationRequested == false)
            {
                _cancellationTokenSource.Cancel();
                _logger.DebugFormat($"[{nameof(SessionHandler)}] CancellationTokenSource canceled.");
            }

            _cancellationTokenSource.Dispose();
            _logger.DebugFormat($"[{nameof(SessionHandler)}] CancellationTokenSource disposed.");
        }
    }
}

