using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WebhooksDataContracts;
using XeroWebhooksReceiver.Helpers;

namespace XeroWebhooksReceiverIntegration.Tests
{
    public static class ContactPayloadGenerator
    {

        public static PayloadSignatureTuple CreateSamplePayload(int numberOfEvents, bool useValidSigningKey, int firstSequenceNumber)
        {
            var sampleEvents = CreateThisManyEvents(numberOfEvents);

            var payload = new Payload
            {
                Events = sampleEvents,
                FirstEventSequence = firstSequenceNumber,
                LastEventSequence = firstSequenceNumber + numberOfEvents - 1
            };

            var payloadString = JsonConvert.SerializeObject(payload);

            var signature = new SignatureVerifier().GenerateSignature(useValidSigningKey 
                ? File.ReadAllText("Resources/SigningKey.txt") 
                : "InvalidKey", payloadString);

            return new PayloadSignatureTuple(payload, signature);
        }

        private static List<Event> CreateThisManyEvents(int numberOfEvents)
        {
            var events = new List<Event>();

            for (var i = 0; i < numberOfEvents; i++)
            {
                var resourceId = Guid.NewGuid();
                
                events.Add(new Event
                {
                    ResourceUrl = $"https://api.xero.com/api.xro/v2/Contacts/{resourceId}",
                    ResourceId = resourceId,
                    EventDateUtc = DateTime.UtcNow,
                    EventType = "CREATE",
                    EventCategory = "CONTACT",
                    TenantId = Guid.NewGuid(),
                    TenantType = "ORGANISATION"
                });
            }

            return events;
        }
    }
}
