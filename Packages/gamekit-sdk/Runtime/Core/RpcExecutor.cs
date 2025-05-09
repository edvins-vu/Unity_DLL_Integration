using System;
using System.Threading.Tasks;
using Estoty.Gamekit.Logger;
using Estoty.GameKit.Utility.Responses;

namespace Estoty.Gamekit.Core
{
	public class RpcExecutor
	{
		private Func<Task<IResponse>> _retryTask;
		private readonly ILogger _logger;

		public RpcExecutor(Func<Task<IResponse>> retryTask, ILogger logger)
		{
			_retryTask = retryTask;
			_logger = logger;
		}

		public async Task<Response> Execute(Func<Task> task)
		{
			Response response = await Send(task);

			if (response.Failed == false)
				return response;
			
			IResponse retryTaskResponse = await _retryTask();

			if (retryTaskResponse.Failed)
				return new Response(GetRetryException(response, retryTaskResponse));
				
			Response retryResponse = await Send(task);
			
			return retryResponse;
		}
		
		public async Task<Response<T>> Execute<T>(Func<Task<T>> task)
		{
			Response<T> response = await Send(task);

			if (response.Failed == false)
			{
				return response;
			}
			
			IResponse retryTaskResponse = await _retryTask();

			_logger.Info($"Execute retryTaskResponse being accessed value: {retryTaskResponse.Failed == false}");

			if (retryTaskResponse.Failed)
			{
				return new Response<T>(GetRetryException(response, retryTaskResponse));
			}
			
			_logger.Info($"Executing Send with retryTaskResponse now.");
			Response<T> retryResponse = await Send(task);

			return retryResponse;
		}

		private async Task<Response<T>> Send<T>(Func<Task<T>> task)
		{
			try
			{
				T payload = await task();
				var returned = new Response<T>(payload);

				_logger.Info($"Received response from server, now casted in Response<T> format from SendRQ: {returned.Payload}");

				return new Response<T>(payload);
			}
			catch (Exception exception)
			{
				_logger.Info($"Exception caught in RPcExecutor SendRQ: {exception.ToString()}");

				_logger.Error(exception.ToString());
				return new Response<T>(exception);
			}
		}
		
		private async Task<Response> Send(Func<Task> task)
		{
			try
			{
				await task();
				return new Response();
			}
			catch (Exception exception)
			{
				return new Response(exception);
			}
		}

		private Exception GetRetryException(IResponse response, IResponse retryTaskResponse)
		{
			string retryException = $"[Retry task exception] \n {retryTaskResponse.Exception} \n";
			string taskException = $"[Task exception] \n {response.Exception}";
			string errorMessage = retryException + taskException;
			
			_logger.Error(errorMessage);
			return new Exception(errorMessage);
		}
	}
}