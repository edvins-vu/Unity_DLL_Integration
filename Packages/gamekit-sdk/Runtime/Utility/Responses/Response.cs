using System;

namespace Estoty.GameKit.Utility.Responses
{
	public readonly struct Response : IResponse
	{
		public Exception Exception { get; }
		public bool Failed => Exception != null;

		public Response(Exception exception)
		{
			Exception = exception;
		}
	}

	public readonly struct Response<T> : IResponse
	{
		public T Payload { get; }
		public Exception Exception { get; }
		public bool Failed => Exception != null;

		public Response(T payload)
		{
			Payload = payload;
			Exception = null;
		}

		public Response(Exception exception)
		{
			Payload = default;
			Exception = exception;
		}
	}
}