using System.Net.Http;
using API_Handler;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts
{
    public class APIService
    {
        private readonly APIManager _apiManager;

        public APIService(APIConfig config)
        {
            HttpClient client = new HttpClient();
            _apiManager = new APIManager(client, config);
        }

        public async UniTask<string> FetchFactsAsync(string endpointKey)
        {
            return await _apiManager.SendRequest(endpointKey, HttpMethod.Get);
        }
    }
}
