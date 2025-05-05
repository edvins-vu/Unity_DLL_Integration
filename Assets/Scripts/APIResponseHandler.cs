using UnityEngine;
using TMPro;
using System.Net.Http;
using API_Handler;
using Assets.Scripts;
using System.Collections.Generic;
//using Newtonsoft.Json.Linq;


public class APIResponseHandler : MonoBehaviour
{
	public TextMeshProUGUI responseText;
	private APIManager _apiManager;

	public async void CallAPI()
	{
		string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, "api_config.json");
		APIConfig config = APIConfig.LoadConfig(configPath);

		HttpClient client = new HttpClient();
		_apiManager = new APIManager(client, config);

		string endPoint = "GetFacts";

		try
		{
			Debug.Log("Calling API...");
			string response = await _apiManager.SendRequest(endPoint, HttpMethod.Get);

			if (response.StartsWith("Error"))
			{
				responseText.text = "Error: " + response;
			}
			else
			{

				ResponseData formattedResponse = JsonUtility.FromJson<ResponseData>(response);
				Fact fact = formattedResponse.data[0];

				responseText.text = fact.attributes.body;
			}
		}
		catch (System.Exception ex)
		{
			responseText.text = "Unexpected error: " + ex.Message;
			Debug.LogError("Encountered an Exception: " + ex.Message);
		}
	}

	public void HandleApiResponse(string jsonResponse, string requestType)
	{
		if (requestType == "fact")
		{
			ApiResponse<Fact> response = JsonUtility.FromJson<ApiResponse<Fact>>(jsonResponse);
			DisplayFacts(response.data);
		}
		//else if (requestType == "breed")
		//{
		//	ApiResponse<Breed> response = JsonUtility.FromJson<ApiResponse<Breed>>(jsonResponse);
		//	DisplayBreeds(response.data);
		//}
		else
		{
			responseText.text = "Error: Unsupported data type.";
		}
	}

	public void DisplayFacts(List<Fact> facts)
	{
		string formattedText = "Facts:\n";

		foreach (Fact fact in facts)
		{
			formattedText += $"- {fact.attributes.body}\n";
		}

		responseText.text = formattedText;
	}

	//private string FormatJsonResponse(string json)
	//{
	//	try
	//	{
	//		JObject formattedJson = JObject.Parse(json);
	//		string responseBody = (string)formattedJson["data"];

	//		return responseBody;
	//	}
	//	catch
	//	{
	//		return "Error: Could not parse response!";
	//	}
	//}
}