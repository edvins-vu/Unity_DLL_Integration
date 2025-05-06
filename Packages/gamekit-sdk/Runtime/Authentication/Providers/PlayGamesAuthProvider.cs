#if UNITY_ANDROID
using System;
using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

namespace Estoty.GameKit.Authentication.Providers
{
	public class PlayGamesAuthProvider : IAuthProvider
	{
		public bool Authenticated => string.IsNullOrEmpty(Token) == false;
		public bool Supported => GooglePlayGames.OurUtils.PlatformUtils.Supported;

		public string Token { get; private set; }
		public string UserId { get; private set; }

		public PlayGamesAuthProvider()
		{
			PlayGamesPlatform.DebugLogEnabled = Debug.isDebugBuild;
			PlayGamesPlatform.Activate();
		}

		public async Task<Response> Authenticate()
		{
			TaskCompletionSource<SignInStatus> signInStatusCompletionSource = new();
			PlayGamesPlatform.Instance.ManuallyAuthenticate(status 
				=> { signInStatusCompletionSource.SetResult(status); });

			SignInStatus status = await signInStatusCompletionSource.Task;

			if (status != SignInStatus.Success)
			{
				string message = $"Authentication failed with status: {status}";
				Exception exception = new(message);
				Debug.LogError(exception);

				return new Response(exception);
			}

			TaskCompletionSource<string> serverAccessTokenCompletionSource = new();
			PlayGamesPlatform.Instance.RequestServerSideAccess(false, 
				token => { serverAccessTokenCompletionSource.SetResult(token); });
			
			string token = await serverAccessTokenCompletionSource .Task;

			if (string.IsNullOrEmpty(token))
			{
				string message = "Failed to retrieve token";
				Exception exception = new(message);
				Debug.LogError(exception);

				return new Response(exception);
			}

			Token = token;
			UserId = PlayGamesPlatform.Instance.GetUserId();

			return new Response();
		}
	}
}
#endif