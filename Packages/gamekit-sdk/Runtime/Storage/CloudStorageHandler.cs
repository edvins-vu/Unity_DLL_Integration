using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Estoty.Gamekit.Core;
using Estoty.GameKit.Utility.Responses;
using Nakama;
using Newtonsoft.Json;
using UnityEngine;

namespace Estoty.Gamekit.Storage
{
	public class CloudStorageHandler : IDisposable
	{
		private ISession _session;
		
		private readonly IClient _client;
		private readonly ISessionHandler _sessionHandler;

		public CloudStorageHandler(
			ISessionHandler sessionHandler,
			IClient client)
		{
			_sessionHandler = sessionHandler;
			_client = client;
			
			_session = _sessionHandler.Session;
			sessionHandler.OnSessionChange += OnSessionChange;
		}

		private void OnSessionChange()
		{
			_session = _sessionHandler.Session;
		}

		public async Task<Response<IApiStorageObjectAcks>> Save(params StorageEntry[] entries)
		{
			Response response = ValidateSession();

			if (response.Failed)
				return new Response<IApiStorageObjectAcks>(response.Exception);

			List<IApiWriteStorageObject> objects = new();

			foreach (StorageEntry entry in entries)
			{
				objects.Add(new WriteStorageObject
				{
					Collection = entry.Collection,
					Key = entry.Key,
					Value = entry.Data
				});
			}

			try
			{
				IApiStorageObjectAcks storageObjectAcks = await _client
					.WriteStorageObjectsAsync(_session, objects.ToArray());

				return new Response<IApiStorageObjectAcks>(storageObjectAcks);
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response<IApiStorageObjectAcks>(exception);
			}
		}

		/// <param name="fileInfo">
		/// A dictionary containing file information:
		/// - The key (`string`): Represents the storage object key.
		/// - The value (`string`): Represents the collection name where the object is stored.
		/// </param>
		public async Task<Response<T[]>> Load<T>(Dictionary<string, string> fileInfo)
		{
			try
			{
				Response<IApiStorageObject[]> response = await Load(fileInfo);

				if (response.Failed)
					return new Response<T[]>(response.Exception);

				T[] receivedData = response.Payload
					.Select(saveObject => JsonConvert.DeserializeObject<T>(saveObject.Value))
					.ToArray();

				return new Response<T[]>(receivedData);
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response<T[]>(exception);
			}
		}

		/// <param name="fileInfo">
		/// A dictionary containing file information:
		/// - The key (`string`): Represents the storage object key.
		/// - The value (`string`): Represents the collection name where the object is stored.
		/// </param>
		public async Task<Response<IApiStorageObject[]>> Load(Dictionary<string, string> fileInfo)
		{
			Response validateResponse = ValidateSession();

			if (validateResponse.Failed)
				return new Response<IApiStorageObject[]>(validateResponse.Exception);

			try
			{
				List<IApiReadStorageObjectId> objectIds = new();

				foreach ((string key, string collection) in fileInfo)
				{
					objectIds.Add(new StorageObjectId
					{
						Collection = collection,
						Key = key,
						UserId = _session.UserId
					});
				}

				IApiStorageObjects payload = await _client.ReadStorageObjectsAsync(_session, objectIds.ToArray());

				return new Response<IApiStorageObject[]>(payload.Objects.ToArray());
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response<IApiStorageObject[]>(exception);
			}
		}

		public async Task<Response<Task>> Delete(StorageObjectId[] ids)
		{
			Response response = ValidateSession();

			if (response.Failed)
				return new Response<Task>(response.Exception);

			foreach (StorageObjectId id in ids)
			{
				id.UserId = _session.UserId;
			}

			try
			{
				Task task = _client.DeleteStorageObjectsAsync(_session, ids);
				await task;

				return new Response<Task>(task);
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return new Response<Task>(exception);
			}
		}

		private Response ValidateSession()
		{
			if (_session.IsExpired == false && _session.IsRefreshExpired == false)
				return new Response();

			string message = "Unexpected session state. Session or refresh token expired.";
			Exception exception = new(message);
			return new Response(exception);
		}

		public void Dispose()
		{
			_sessionHandler.OnSessionChange -= OnSessionChange;
		}
	}
}