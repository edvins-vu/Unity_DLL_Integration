using System;
using System.Threading.Tasks;
using Estoty.GameKit.Authentication.Providers;
using Estoty.Gamekit.Core;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using UnityEngine;
using ILogger = Estoty.Gamekit.Logger.ILogger;
using IAuthProvider = Estoty.GameKit.Authentication.Providers.IAuthProvider;

namespace Estoty.GameKit.Authentication
{
	public class AccountLinkingHandler
	{
		private readonly ILogger _logger;
		private readonly IClient _client;
		private readonly IAuthProvider _authProvider;
		private readonly ISessionHandler _sessionHandler;

		private ISession Session => _sessionHandler.Session;
		private IApiAccount Account => _sessionHandler.Account;

		public AccountLinkingHandler(
			ILogger logger,
			IClient client,
			IAuthProvider authProvider,
			ISessionHandler sessionHandler)
		{
			_logger = logger;
			_client = client;
			_authProvider = authProvider;
			_sessionHandler = sessionHandler;
		}

		public async Task<Response> LinkAsync()
		{
			try
			{
				switch (_authProvider)
				{
#if UNITY_ANDROID
					case PlayGamesAuthProvider playGamesIdentity:
						Response response = await playGamesIdentity.Authenticate();

						if (response.Failed)
							return response;

						string userGoogleID = Account.User.GoogleId;

						if (string.IsNullOrEmpty(userGoogleID) == false || userGoogleID == playGamesIdentity.UserId)
						{
							string messageGoogle = $"The account is already linked to a Google account: {userGoogleID}";
							_logger.Error(messageGoogle);
							return new Response(new Exception(messageGoogle));
						}

						await _client.LinkGoogleAsync(Session, playGamesIdentity.Token);
						await UnlinkDevices();
						break;
#elif UNITY_IOS
					case GameCenterAuthProvider gameCenterAuthProvider:
						Response response = await gameCenterAuthProvider.Authenticate();

						if (response.Failed)
							return response;

						string gameCenterId = Account.User.GamecenterId;
						GameCenterCredentials credentials = gameCenterAuthProvider.Credentials;

						if (string.IsNullOrEmpty(gameCenterId) == false || credentials.PlayerId == gameCenterId)
						{
							return new Response(
								new Exception($"The account is already linked to a Game Center account: {gameCenterId}"));
						}

						await _client.LinkGameCenterAsync(
							Session,
							credentials.BundleId,
							credentials.PlayerId,
							credentials.PublicKeyUrl,
							credentials.Salt,
							credentials.Signature,
							credentials.Timestamp
						);

						if (string.IsNullOrEmpty(Account.User.DisplayName) == false)
							break;

						await _client.UpdateAccountAsync(_sessionHandler.Session, Account.User.Username, credentials.DisplayName);
						await UnlinkDevices();
						break;
#endif
					default:
						string machineName = "[Editor] " + Environment.MachineName;
						await _client.UpdateAccountAsync(Session, Account.User.Username, machineName);
						break;
				}
			}
			catch (Exception exception)
			{
				_logger.Error("Failed to link account: " + exception);
				return new Response(exception);
			}

			await _sessionHandler.RefreshAccount();

			return new Response();
		}

		public async Task<Response<bool>> IsAccountLinked()
		{
			try
			{
				switch (_authProvider)
				{
#if UNITY_ANDROID
					case PlayGamesAuthProvider:
						string userGoogleID = Account.User.GoogleId;
						return new Response<bool>(string.IsNullOrEmpty(userGoogleID) == false);
#elif UNITY_IOS
					case GameCenterAuthProvider gameCenterAuthProvider:
						string gameCenterId = Account.User.GamecenterId;
						return new Response<bool>(string.IsNullOrEmpty(gameCenterId) == false);
#endif
					default:
						IApiAccount account = await _client.GetAccountAsync(Session);
						return new Response<bool>(account.User.DisplayName == Environment.MachineName);
				}
			}
			catch (Exception exception)
			{
				_logger.Error("Failed to check if account is linked: " + exception);
				return new Response<bool>(exception);
			}
		}

		private async Task UnlinkDevices()
		{
			try
			{
				foreach (IApiAccountDevice device in Account.Devices)
				{
					await _client.UnlinkDeviceAsync(Session, device.Id);
				}
			}
			catch (Exception exception)
			{
				_logger.Error("Failed to unlink devices: " + exception.Message);
			}
		}
	}
}