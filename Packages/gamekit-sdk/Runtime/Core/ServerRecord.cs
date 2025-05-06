using UnityEngine;

namespace Estoty.Gamekit.Core
{
	[CreateAssetMenu(menuName = Constants.ROOT + "/Records/" + nameof(ServerRecord), fileName = nameof(ServerRecord))]
	public class ServerRecord : ScriptableObject
	{
		public ServerConfigRecord Config
		{
			get
			{
#if PRODUCTION
				return Production;
#elif DEVELOPMENT_BUILD
				return Development;
#elif UNITY_EDITOR
				return UseLocal ? Local : Development;
#endif
			}
		}

		[field: SerializeField] 
		public ServerConfigRecord Local { get; private set; }

		[field: SerializeField] 
		public ServerConfigRecord Development { get; private set; }

		[field: SerializeField] 
		public ServerConfigRecord Production { get; private set; }

		[field: Space, SerializeField] 
		public bool UseLocal { get; private set; }
	}
}