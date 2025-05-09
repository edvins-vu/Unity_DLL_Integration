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
	public const string idToFetch = "dbe98608-9de9-4983-8570-df034056935e";

	async void Start()
	{
		await InitializeGamekitSDK();
		MessageResponse response = await FetchNotification();

		Debug.Log($"SUCCESS: MESSAGE READ: ID: {response.Messages[0].Id}, Content: {response.Messages[0].Content.MessageText}," +
			$"CreateTime (seconds): {response.Messages[0].CreateTime.Seconds}");
	}

	private async Task InitializeGamekitSDK()
	{
		if (Uri.TryCreate(serverURL, UriKind.Absolute, out var uri))
		{
			Debug.Log($"MESSAGE ID  DURING INITIALIZE of GamekitSDK: {idToFetch}");
			_gamekitClient = new GamekitSDK(uri.Host, serverPort, apiKey);
			await _gamekitClient.SessionHandler.AttemptRestoreSession();
		}
		else
		{
			Debug.LogError($"Invalid server URL: {serverURL}");
			return;
		}
	}
	public async Task<MessageResponse> FetchNotification()
	{
		if (_gamekitClient != null && _gamekitClient.SessionHandler != null && _gamekitClient.SessionHandler.SessionValid)
		{
			var mailResponse = await _gamekitClient.ReadNotification(idToFetch);

			if (mailResponse.Failed)
			{
				Debug.LogError($"[GamekitManager] Failed to get message: {mailResponse.Exception}");
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

	public async Task<MailboxResponse> FetchMail()
	{
		if (_gamekitClient != null && _gamekitClient.SessionHandler != null && _gamekitClient.SessionHandler.SessionValid)
		{
			var mailResponse = await _gamekitClient.GetMail(idToFetch);

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

	private void OnDestroy()
	{
		_gamekitClient?.Dispose();
	}
}