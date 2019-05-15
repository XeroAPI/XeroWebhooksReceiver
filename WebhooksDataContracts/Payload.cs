using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebhooksDataContracts
{
    public class Payload
    {
        [JsonProperty("events")]
        public List<Event> Events { get; set; }

        [JsonProperty("firstEventSequence")]
        public int FirstEventSequence { get; set; }

        [JsonProperty("lastEventSequence")]
        public int LastEventSequence { get; set; }
    }
}
