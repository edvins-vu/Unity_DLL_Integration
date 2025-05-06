using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Estoty.GameKit.Authentication.Providers;
using Estoty.Gamekit.Utility.PlayerPrefs;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using UnityEngine;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}

namespace Estoty.GameKit.Authentication
{
	public class AuthHandler : IAuthHandler
	{
		private bool ProviderAuth
		{
			get => PlayerPrefsUtility.GetBool(AuthConstants.PLAYER_PREFS_PROVIDER_AUTH);
			set => PlayerPrefsUtility.SetBool(AuthConstants.PLAYER_PREFS_PROVIDER_AUTH, value);
		}

		private string AuthToken => PlayerPrefs.GetString(AuthConstants.PLAYER_PREFS_AUTH_TOKEN);
		private string RefreshToken => PlayerPrefs.GetString(AuthConstants.PLAYER_PREFS_REFRESH_TOKEN);

		private Dictionary<string, string> _sessionVariables;

		private string _appVersion;
		private readonly ILogger _logger;
		private readonly IClient _client;
		private readonly IAuthProvider _authProvider;

		public AuthHandler(
			string appVersion,
			IClient client,
			ILogger logger,
			IAuthProvider authProvider)
		{
			_appVersion = appVersion;
			_authProvider = authProvider;
			_logger = logger;
			_client = client;
		}

		public async Task<Response<ISession>> Restore(string deviceId)
		{
			try
			{
				if (string.IsNullOrEmpty(AuthToken) && string.IsNullOrEmpty(RefreshToken))
					return await AuthenticateDeviceAsync(deviceId);

				ISession session = Session.Restore(AuthToken, RefreshToken);

				if (session.IsExpired)
				{
					return ProviderAuth ? await LoginAsync() : await AuthenticateDeviceAsync(deviceId);
				}

				session = await _client.SessionRefreshAsync(session, GetSessionVariables());

				UpdateTokens(session);
				return new Response<ISession>(session);
			}
			catch (ApiResponseException exception)
			{
				const int STATUS_CODE_NOT_FOUND = 404;
				const int STATUS_CODE_UNAUTHORIZED = 401;

				if (exception.StatusCode is STATUS_CODE_NOT_FOUND or STATUS_CODE_UNAUTHORIZED)
					return await AuthenticateDeviceAsync(deviceId);

				_logger.Error($"[{nameof(AuthHandler)}] Session restore failed: " + exception);
				return new Response<ISession>(exception);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(AuthHandler)}] Session restore failed: " + exception);
				return new Response<ISession>(exception);
			}
		}

		public async Task<Response> LogoutAsync(ISession session)
		{
			try
			{
				await _client.SessionLogoutAsync(session);

				PlayerPrefs.DeleteKey(AuthConstants.PLAYER_PREFS_AUTH_TOKEN);
				PlayerPrefs.DeleteKey(AuthConstants.PLAYER_PREFS_REFRESH_TOKEN);

				return new Response();
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(AuthHandler)}] Logout failed: " + exception);
				return new Response(exception);
			}
		}

		public async Task<Response<ISession>> LoginAsync()
		{
			try
			{
				Response response = await _authProvider.Authenticate();

				if (response.Failed)
				{
					_logger.Error($"[{nameof(AuthHandler)}] User login failed: " + response);
					return new Response<ISession>(response.Exception);
				}

				Response<ISession> sessionResponse = await Login(_authProvider, GetSessionVariables());

				if (sessionResponse.Failed)
				{
					_logger.Error($"[{nameof(AuthHandler)}] User login failed: " + sessionResponse.Exception);
					return new Response<ISession>(sessionResponse.Exception);
				}

				ISession session = sessionResponse.Payload;
				UpdateTokens(session);
				ProviderAuth = true;
				return new Response<ISession>(session);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(AuthHandler)}] User login failed: " + exception);
				return new Response<ISession>(exception);
			}
		}

		private async Task<Response<ISession>> Login(
			IAuthProvider authProvider,
			Dictionary<string, string> sessionVariables = default
		)
		{
			ISession session;

			if (authProvider.Authenticated == false)
			{
				string message = "Authentication failed: The user is not authenticated.";
				Exception exception = new(message);
				Debug.LogError(exception);

				return new Response<ISession>(exception);
			}

			try
			{
				switch (authProvider)
				{
#if UNITY_ANDROID
					case PlayGamesAuthProvider provider:
						session = await _client.AuthenticateGoogleAsync(provider.Token, vars: sessionVariables);

						if (session != null)
							break;

						Exception exception = new("Failed to authenticate with Google Play Games.");
						Debug.LogError(exception);
						return new Response<ISession>(exception);
#elif UNITY_IOS
					case GameCenterAuthProvider provider:
						GameCenterCredentials credentials = provider.Credentials;
						
						session = await _client.AuthenticateGameCenterAsync(
							credentials.BundleId,
							credentials.PlayerId,
							credentials.PublicKeyUrl,
							credentials.Salt,
							credentials.Signature,
							credentials.Timestamp,
							vars: sessionVariables
						);

						if (session == null)
						{
							Exception exception = new("Failed to authenticate with Game Center.");
							Debug.LogError(exception);
							return new Response<ISession>(exception);
						}
						
						IApiAccount account = await _client.GetAccountAsync(session);
						
						if (account == null)
						{
							Exception exception = new("Failed to retrieve account information after Game Center authentication.");
							Debug.LogError(exception);
							return new Response<ISession>(exception);
						}

						if (string.IsNullOrEmpty(account.User.DisplayName) == false)
							break;

						await _client.UpdateAccountAsync(session, account.User.Username, credentials.DisplayName);
						break;
#endif
					default: 
						session = await _client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier, vars: sessionVariables);

						if (session != null)
							break;

						Exception exceptionDevice = new("Failed to authenticate with device ID.");
						Debug.LogError(exceptionDevice);
						return new Response<ISession>(exceptionDevice);
				}

				return new Response<ISession>(session);
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response<ISession>(exception);
			}
		}

		private async Task<Response<ISession>> AuthenticateDeviceAsync(string deviceId)
		{
			try
			{
				ISession session = await _client.AuthenticateDeviceAsync(deviceId, vars: GetSessionVariables());
				UpdateTokens(session);
				ProviderAuth = false;
				
				#if UNITY_EDITOR
				IApiAccount account = await _client.GetAccountAsync(session);
				await _client.UpdateAccountAsync(session, account.User.Username, $"[Editor-Unlinked] {Environment.MachineName}");
				#endif
				
				return new Response<ISession>(session);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(AuthHandler)}] Device authentication failed: " + exception);
				return new Response<ISession>(exception);
			}
		}

		private void UpdateTokens(ISession session)
		{
			PlayerPrefs.SetString(AuthConstants.PLAYER_PREFS_AUTH_TOKEN, session.AuthToken);
			PlayerPrefs.SetString(AuthConstants.PLAYER_PREFS_REFRESH_TOKEN, session.RefreshToken);
		}

		private Dictionary<string, string> GetSessionVariables()
		{
			if (_sessionVariables != null)
				return _sessionVariables;

			Version version = new(_appVersion);

			const string DEVICE_ID = "device_id";
			const string VERSION = "version";
			const string TEST_GROUP = "test_group";

			_sessionVariables = new Dictionary<string, string>
			{
				{DEVICE_ID, SystemInfo.deviceUniqueIdentifier},
				{VERSION, $"{version.Major}.{version.Minor}.{version.Build}"},
				{TEST_GROUP, version.Revision == -1 ? string.Empty : version.Revision.ToString()}
			};

			return _sessionVariables;
		}
	}
}