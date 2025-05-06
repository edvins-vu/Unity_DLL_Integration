#if UNITY_IOS
using System;
using System.Threading.Tasks;
using Apple.GameKit;
using Apple.GameKit.Players;
using Estoty.GameKit.Utility.Responses;
using UnityEngine;

namespace Estoty.GameKit.Authentication.Providers
{
	public class GameCenterAuthProvider : IAuthProvider
	{
		public bool Authenticated => Credentials.Equals(default) == false;
		public bool Supported => true;

		public GameCenterCredentials Credentials { get; private set; }

		public async Task<Response> Authenticate()
		{
			try
			{
				if (!GKLocalPlayer.Local.IsAuthenticated) 
					await GKLocalPlayer.Authenticate();

				GKLocalPlayer localPlayer = GKLocalPlayer.Local;
				GKIdentityVerificationResponse fetchItemsResponse = await GKLocalPlayer.Local.FetchItems();

				Credentials = new GameCenterCredentials()
				{
					BundleId = Application.identifier,
					PlayerId = localPlayer.TeamPlayerId,
					DisplayName = localPlayer.DisplayName,
					PublicKeyUrl = fetchItemsResponse.PublicKeyUrl,
					Salt = Convert.ToBase64String(fetchItemsResponse.GetSalt()),
					Signature = Convert.ToBase64String(fetchItemsResponse.GetSignature()),
					Timestamp = fetchItemsResponse.Timestamp.ToString()
				};
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response(exception);
			}

			return new Response();
		}
	}
}
#endif