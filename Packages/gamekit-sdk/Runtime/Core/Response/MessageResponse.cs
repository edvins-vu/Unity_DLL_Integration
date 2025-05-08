using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Estoty.Gamekit.Core
{
	public class MessageResponse
	{
		[JsonProperty("messages")]
		public List<Message> Messages { get; set; }

	}
	public class Message
	{
		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("UserID")]
		public string UserID { get; set; }

		[JsonProperty("Subject")]
		public string Subject { get; set; }

		[JsonProperty("Content")]
		public Content Content { get; set; }

		[JsonProperty("Code")]
		public int Code { get; set; }

		[JsonProperty("Sender")]
		public string Sender { get; set; }

		[JsonProperty("CreateTime")]
		public CreateTime CreateTime { get; set; }

		[JsonProperty("Persistent")]
		public bool Persistent { get; set; }
	}

	public class Content
	{
		[JsonProperty("message")]
		public string MessageText { get; set; }

		[JsonProperty("resources")]
		public Resources Resources { get; set; }
	}

	public class Resources
	{
		[JsonProperty("currencies")]
		public Dictionary<string, int> Currencies { get; set; }
	}

	public class Currencies
	{
		[JsonProperty("ResourceGoldCoin")]
		public int ResourceGoldCoin { get; set; }
	}

	public class CreateTime
	{
		[JsonProperty("seconds")]
		public long Seconds { get; set; }
	}
}