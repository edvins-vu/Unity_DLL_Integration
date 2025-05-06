using UnityEngine;

namespace Estoty.Gamekit.Logger
{
	public interface ILogger : Nakama.ILogger
	{
		void Info(string message, Object context = null);
		void Warning(string message, Object context = null);
		void Error(string message, Object context = null);
	}
}