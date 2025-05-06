using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoty.Gamekit.Core;
using Estoty.Gamekit.User.Payloads;
using Nakama;
using Newtonsoft.Json;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace Estoty.Gamekit.User
{
	public class UserHandler
	{
		private readonly ILogger _logger;
		private readonly ISessionHandler _sessionHandler;
		private readonly IRpcHandler _rpcHandler;

		public UserHandler(
			ILogger logger,
			IRpcHandler rpcHandler,
			ISessionHandler sessionHandler
		)
		{
			_rpcHandler = rpcHandler;
			_sessionHandler = sessionHandler;
			_logger = logger;
		}
		
		public async Task UpdateMetadata(UserMetadata request)
		{
			try
			{
				UserMetadata metadata = GetMetadata();

				bool isOverride = metadata.Override ?? false;
				IDictionary<string, string> sessionVariables = metadata.LastSession ?? new Dictionary<string, string>();

				if (request.Override.HasValue)
				{
					isOverride = request.Override.Value;
				}

				if (request.LastSession?.Any() == true)
				{
					sessionVariables = request.LastSession;
				}
					
				UserMetadata payload = new()
				{
					Override = isOverride,
					LastSession = sessionVariables
				};

				await _rpcHandler.SendRequest(UserConstants.UPDATE_METADATA, payload);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(UserHandler)}] {nameof(UpdateMetadata)} error : {exception}");
			}
		}

		public UserMetadata GetMetadata()
		{
			try
			{
				IApiAccount account = _sessionHandler.Account;
				return account == null ? default : JsonConvert.DeserializeObject<UserMetadata>(account.User.Metadata);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(UserHandler)}] {nameof(GetMetadata)} error : {exception}");
			}

			return default;
		}

		public async void UpdateSessionVariables()
		{
			try
			{
				UserMetadata request = new()
				{
					LastSession = _sessionHandler.Session.Vars
				};

				await UpdateMetadata(request);
			}
			catch (Exception exception)
			{
				_logger.Error($"[{nameof(UserHandler)}] {nameof(UpdateSessionVariables)} error : {exception}");
			}
		}
	}
}