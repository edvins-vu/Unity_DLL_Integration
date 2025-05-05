using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;
using API_Handler;

namespace Assets.Scripts
{
    public class APIResponseHandler : MonoBehaviour
    {
        public Button callApiButton;
        public FactUIHandler factUIHandler;
        private APIService _apiService;

        private string endPoint = "GetFacts";

        private void Start()
        {
            callApiButton.onClick.AddListener(CallAPIWrapper);

            // Load API configuration and initialize APIService
            string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, "api_config.json");
            APIConfig config = APIConfig.LoadConfig(configPath);

            _apiService = new APIService(config);
        }

        public void CallAPIWrapper()
        {
            CallAPI().Forget();
        }

        public async UniTask CallAPI()
        {
            string response = await _apiService.FetchFactsAsync(endPoint);

            if (response.StartsWith("Error"))
            {
                factUIHandler.responseText.text = "Error: " + response;
            }
            else
            {
                ResponseData formattedResponse = JsonUtility.FromJson<ResponseData>(response);
                factUIHandler.UpdateFacts(formattedResponse.data);
            }
        }
    }
}