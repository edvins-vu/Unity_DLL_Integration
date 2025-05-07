using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Estoty.Gamekit.Core
{
    public class MailboxResponse
    {
        [JsonProperty("next_cursor")]
        public int NextCursor { get; set; }

        [JsonProperty("notifications")]
        public List<Notification> Notifications { get; set; }
    }

    public class Notification
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
		public string UserId { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("content")]
        public NotificationContent Content { get; set; }

		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonProperty("sender_id")]
		public string SenderId { get; set; }

		[JsonProperty("create_time")]
        public DateTime CreateTime { get; set; }
    }

    public class NotificationContent
	{
		[JsonProperty("message")]
		public string Message { get; set; }
		[JsonProperty("resources")]
		public ContentResources Resources { get; set; }
	}

	public class ContentResources
	{
        [JsonProperty("currencies")]
		public Dictionary<string, int> Currencies { get; set; }
	}
}