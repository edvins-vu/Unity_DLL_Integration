using System.Net.Http;
using API_Handler;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class APIService
    {
        private readonly APIManager _apiManager;

        // Send the request using the method from Api_Handler DLL
        public APIService(APIConfig config)
        {
            HttpClient client = new HttpClient();
            _apiManager = new APIManager(client, config);
        }

        public async UniTask<string> FetchFactsAsync(string endpointKey)
        {
            return await _apiManager.SendRequest(endpointKey, HttpMethod.Get);
        }

        //public void HandleApiResponse(string jsonResponse, string requestType)
        //{
        //    if (requestType == "fact")
        //    {
        //        ApiResponse<Fact> response = JsonUtility.FromJson<ApiResponse<Fact>>(jsonResponse);
        //        DisplayFacts(response.data);
        //    }
        //    //else if (requestType == "breed")
        //    //{
        //    //	ApiResponse<Breed> response = JsonUtility.FromJson<ApiResponse<Breed>>(jsonResponse);
        //    //	DisplayBreeds(response.data);
        //    //}
        //    else
        //    {
        //        responseText.text = "Error: Unsupported data type.";
        //    }
        //}
    }
}
