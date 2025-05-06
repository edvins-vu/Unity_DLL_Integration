using System;
using ILogger = Estoty.Gamekit.Logger.ILogger;

namespace Estoty.Gamekit.Core
{
    // Basic default logger implementation
    public class DefaultLogger : ILogger
    {
        public void Debug(string message) => UnityEngine.Debug.Log($"[Debug] {message}");
        public void Info(string message, UnityEngine.Object context = null) => UnityEngine.Debug.Log($"[Info] {message}");
        public void Warning(string message, UnityEngine.Object context = null) => UnityEngine.Debug.Log($"[Warning] {message}");
        public void Error(string message, UnityEngine.Object context = null) => UnityEngine.Debug.LogError($"[Error] {message}");
        public void Exception(Exception exception, UnityEngine.Object context = null) => UnityEngine.Debug.LogException(exception);

        public void DebugFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat($"[Debug] {format}", args);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat($"[Error] {format}", args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat($"[Info] {format}", args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat($"[Warning] {format}", args);
        }
    }
}