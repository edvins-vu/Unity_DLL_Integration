using UnityEngine;

namespace Estoty.Gamekit.Core
{
	[CreateAssetMenu(menuName = Constants.ROOT + "/Records/" + nameof(ServerConfigRecord), fileName = nameof(ServerConfigRecord))]
	public class ServerConfigRecord : ScriptableObject
	{
		private static ServerConfigRecord _instance;
		public static ServerConfigRecord Instance
		{
			get 
			{
				if (_instance == null)
				{
					_instance = Resources.Load<ServerConfigRecord>(nameof(ServerConfigRecord));

					if (_instance == null)
					{
						_instance = CreateInstance<ServerConfigRecord>();
						Debug.LogError($"No {{nameof(ServerConfigRecord)}} asset found in Resources. A new one was created at runtime.");
					}
				}
				return _instance;
			}
		}

		[field: SerializeField]
		public string Protocol { get;  set; }

		[field: SerializeField]
		public string Host { get;  set; }

		[field: SerializeField]
		public int Port { get;  set; }
		
		[field: SerializeField]
		public string Key { get;  set; }
	}
}