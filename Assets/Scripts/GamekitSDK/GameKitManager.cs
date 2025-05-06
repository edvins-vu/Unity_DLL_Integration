using UnityEngine;
using System.Threading.Tasks;
using Estoty.Gamekit.Core;

public class GamekitManager : MonoBehaviour
{
    private GamekitSDK _gamekitClient;

    // Set in the Inspector?
    public string serverURL = "YOUR_URL";
    public string serverPort = "YOUR_PORT";
    public string apiKey = "YOUR_X_API_KEY";
    public string appVersion = "YOUR_APP_VERSION";
    public string userIdToFetch = "USER_ID_TO_FETCH";

    async void Start()
    {
        // Initialize the GamekitSDK
        _gamekitClient = new GamekitSDK(serverURL, serverPort, apiKey, appVersion);

        // Try to fetch mail on start
        await FetchMail();
    }

    // Example method to trigger fetching mail from other parts of your code
    public async Task<MailboxResponse> FetchMail()
    {
        if (_gamekitClient != null)
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