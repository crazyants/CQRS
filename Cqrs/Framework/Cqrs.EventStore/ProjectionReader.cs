﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Cqrs.EventStore
{
	public abstract class ProjectionReader
	{
		protected IEventStoreConnectionHelper EventStoreConnectionHelper { get; set; }

		protected IEventDeserialiser EventDeserialiser { get; set; }

		protected ProjectionReader(IEventStoreConnectionHelper eventStoreConnectionHelper, IEventDeserialiser eventDeserialiser)
		{
			EventStoreConnectionHelper = eventStoreConnectionHelper;
			EventDeserialiser = eventDeserialiser;
		}

		protected IEnumerable<dynamic> GetDataByStreamName(string streamName)
		{
			StreamEventsSlice eventCollection;
			using (EventStoreConnection connection = EventStoreConnectionHelper.GetEventStoreConnection())
			{
				eventCollection = connection.ReadStreamEventsBackward(streamName, StreamPosition.End, 1, false);
			}
			var jsonSerialiserSettings = EventDeserialiser.GetSerialisationSettings();
			var encoder = new UTF8Encoding();
			return ((IEnumerable<dynamic>)eventCollection.Events
				.Select(e => JsonConvert.DeserializeObject(((dynamic)encoder.GetString(e.Event.Data)), jsonSerialiserSettings))
				.Single()
				).Select(x => x.Value);
		}

		protected IEnumerable<TData> GetDataByStreamName<TData>(string streamName)
		{
			IList<TData> data = GetDataByStreamName(streamName)
				.Select(e => JsonConvert.DeserializeObject<TData>(e.ToString()))
				.Cast<TData>()
				.ToList();
			return data;
		}
	}
}