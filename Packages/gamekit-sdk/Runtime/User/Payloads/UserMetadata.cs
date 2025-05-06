using System.Collections.Generic;
using Newtonsoft.Json;

namespace Estoty.Gamekit.User.Payloads
{
	public struct UserMetadata
	{
		[JsonProperty("override")]
		public bool? Override { get; set; }

		[JsonProperty("last_session")]
		public IDictionary<string, string> LastSession { get; set; }
	}
}