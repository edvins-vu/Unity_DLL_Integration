using System;
using System.Threading;
using System.Threading.Tasks;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using Newtonsoft.Json;
using UnityEngine.Assertions;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace Estoty.Gamekit.Core
{
	public class RpcHandler : IRpcHandler
	{
		private readonly ILogger _logger;
		private readonly IClient _client;
		private readonly RpcExecutor _rpcExecutor;
		private readonly ISessionHandler _sessionHandler;
		private readonly CancellationToken _cancellationToken;
		
		public RpcHandler(
			ILogger logger,
			IClient client,
			ISessionHandler sessionHandler,
			CancellationToken cancellationToken
		)
		{
			_client = client;
			_sessionHandler = sessionHandler;
			_cancellationToken = cancellationToken;
			_logger = logger;
			_rpcExecutor = new RpcExecutor(_sessionHandler.RestoreSession, logger);
		}

		public Task<Response<T>> SendRequest<T>(string endpoint)
		{
			return SendResponseRequestInternal<T>(endpoint, null);
		}

		public Task<Response> SendRequest(string endpoint, object payload)
		{
			return SendRequestInternal(endpoint, payload);
		}

		public Task<Response<T>> SendRequest<T>(string endpoint, object payload)
		{
			_logger.DebugFormat($"Calling the RpcHandler SendRQ ({endpoint}), {payload.ToString()}");
			return SendResponseRequestInternal<T>(endpoint, payload);
		}

		private async Task<Response> SendRequestInternal(string endpoint, object payload)
		{
			Assert.IsFalse(string.IsNullOrEmpty(endpoint), "Endpoint not defined");

			try
			{
				Response<IApiRpc> response = await _rpcExecutor.Execute(() => RpcAsync(endpoint, payload));
				_cancellationToken.ThrowIfCancellationRequested();

				if (response.Failed == false) 
					return new Response();
				
				LogError(endpoint, payload, response.Exception);
				return new Response(response.Exception);
			}
			catch (OperationCanceledException exception)
			{
				throw exception;
			}
			catch (Exception exception)
			{
				LogError(endpoint, payload, exception);
				return new Response(exception);
			}
		}

		private async Task<Response<T>> SendResponseRequestInternal<T>(string endpoint, object payload)
		{
			Assert.IsFalse(string.IsNullOrEmpty(endpoint), "Endpoint not defined");
		 
			try
			{
				Response<IApiRpc> response = await _rpcExecutor.Execute(() => RpcAsync(endpoint, payload));
				_logger.DebugFormat($"Response from _rpcExecutor.Execute() - Failed: {response.Failed}, Exception: {response.Exception}, Payload: {response.Payload?.Payload}");

				_cancellationToken.ThrowIfCancellationRequested();
				
				if (response.Failed)
				{
					LogError(endpoint, payload, response.Exception);
					return new Response<T>(response.Exception);
				}

				T responsePayload = JsonConvert.DeserializeObject<T>(response.Payload.Payload);
				return new Response<T>(responsePayload);
			}
			catch (OperationCanceledException exception)
			{
				throw exception;
			}
			catch (Exception exception)
			{
				LogError(endpoint, payload, exception);
				return new Response<T>(exception);
			}
		}

		private Task<IApiRpc> RpcAsync(string endpoint, object payload)
		{
			if (payload == null)
			{
				_logger.DebugFormat($"RpcHandler RpcAsync - Payloaad is nukll");
				return _client.RpcAsync(_sessionHandler.Session, endpoint, canceller: _cancellationToken);
			}
			
			string payloadJson = JsonConvert.SerializeObject(payload);
			_logger.DebugFormat($"RpcHandler RpcAsync - Payload is not null");
			return _client.RpcAsync(_sessionHandler.Session, endpoint, payloadJson, canceller: _cancellationToken);
		}

		private void LogError(string endpoint, object payload, Exception exception)
		{
			string message = $"RPC Failed ({endpoint})";

			if (payload.Equals(default) == false)
			{
				message += $", payload:\n{payload}";
			}

			message += "\nException:\n" + exception;
			_logger.Error(message);
		}
	}
}