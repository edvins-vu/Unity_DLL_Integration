using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Estoty.Gamekit.Core
{
    public class MailboxResponse
    {
        [JsonProperty("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("senderId")]
        public string SenderId { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("sentAt")]
        public DateTime SentAt { get; set; }
    }
}