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
	
		private string DeviceId
		{
			get
			{
				string deviceId = PlayerPrefs.GetString(AuthConstants.PLAYER_PREFS_DEVICE_ID);

				if (string.IsNullOrEmpty(deviceId) == false) 
					return deviceId;
				
				deviceId = SystemInfo.deviceUniqueIdentifier;
				PlayerPrefs.SetString(AuthConstants.PLAYER_PREFS_DEVICE_ID, deviceId);

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
			Response response = await _authHandler.LogoutAsync(Session);

			if (response.Failed == false)
			{
				Session = null;
			}
			
			return response;
		}

		public async Task<Response> RefreshAccount()
		{
			try
			{
				Account = await _client.GetAccountAsync(Session, canceller: CancellationToken);
				return new Response();
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(SessionHandler)}] Failed retrieve account: {exception}");
				return new Response(exception);
			}
		}

		public async Task<IResponse> RestoreSession()
		{
			Response<ISession> response = await _authHandler.Restore(DeviceId);
			
			if (response.Failed)
			{
				return new Response(response.Exception);
			}
			
			Session = response.Payload;

			Response responseAccount = await RefreshAccount();

			if (responseAccount.Failed)
			{
				Session = null;
				return new Response(response.Exception);
			}
			
			OnSessionChange?.Invoke();
			return new Response();
		}
		
		public async Task AttemptRestoreSession()
		{
			try
			{
				IResponse response = await RestoreSession();

				if (Initialized == false)
				{
					Initialized = true;
					OnInitialized?.Invoke();
				}

				if (response.Failed == false)
				{
					return;
				}

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
			if (CancellationToken.IsCancellationRequested == false)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource.Dispose();
		}
	}
}