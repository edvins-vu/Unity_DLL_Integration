using System;

namespace Estoty.GameKit.Utility.Responses
{
	public interface IResponse
	{
		public Exception Exception { get; }
		public bool Failed { get; }
	}
}