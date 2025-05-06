using UnityEngine;

namespace Estoty.Gamekit.Core
{
	[CreateAssetMenu(menuName = Constants.ROOT + "/Records/" + nameof(ServerConfigRecord), fileName = nameof(ServerConfigRecord))]
	public class ServerConfigRecord : ScriptableObject
	{
		[field: SerializeField]
		public string Protocol { get; private set; }

		[field: SerializeField]
		public string Host { get; private set; }

		[field: SerializeField]
		public int Port { get; private set; }
		
		[field: SerializeField]
		public string Key { get; private set; }
	}
}