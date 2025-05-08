using UnityEngine;
using System.Threading.Tasks;
using Estoty.Gamekit.Core;
using System;

public class GamekitManager : MonoBehaviour
{
	private GamekitSDK _gamekitClient;

	// Set in the Inspector?
	public string serverURL = "http://localhost";
	public string serverPort = "7350";
	public string apiKey = "defaultkey";
	public string userIdToFetch = "00000000-0000-0000-0000-000000000000";

	async void Start()
	{
		InitializeGamekitSDK();
		await FetchMail();
	}

	private async void InitializeGamekitSDK()
	{
		if (Uri.TryCreate(serverURL, UriKind.Absolute, out var uri))
		{
			_gamekitClient = new GamekitSDK(uri.Host, serverPort, apiKey); // Pass just the host part
			await _gamekitClient.SessionHandler.AttemptRestoreSession(); // Attempt to restore the session
		}
		else
		{
			Debug.LogError($"Invalid server URL: {serverURL}");
			return;
		}
	}

	// Example method to trigger fetching mail
	public async Task<MailboxResponse> FetchMail()
	{
		//Debug.LogError("Session status: " + _gamekitClient != null);
		//Debug.LogError("Session: " + _gamekitClient.SessionHandler != null);

		if (_gamekitClient != null && _gamekitClient.SessionHandler != null) //&& _gamekitClient.SessionHandler.SessionValid)
		{
			var mailResponse = await _gamekitClient.GetMail(userIdToFetch);

			if (mailResponse.Failed)
			{
				Debug.LogError($"[GamekitManager] Failed to get mail: {mailResponse.Exception}");
				return null;
			}
			else
			{
				return mailResponse.Payload;
			}
		}
		else
		{
			Debug.LogError("[GamekitManager] GamekitSDK client is not initialized.");
			return null;
		}
	}

	// Ensure proper disposal when the GameObject is destroyed
	private void OnDestroy()
	{
		_gamekitClient?.Dispose();
	}
}