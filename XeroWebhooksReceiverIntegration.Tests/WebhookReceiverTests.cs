using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebhooksDataContracts;
using XeroWebhooksReceiver;
using XeroWebhooksReceiver.Config;
using XeroWebhooksReceiver.Helpers;
using XeroWebhooksReceiver.Queue;
using Xunit;

namespace XeroWebhooksReceiverIntegration.Tests
{
    public class WebhookReceiverTests : IDisposable
    {
        private readonly PayloadQueue _payloadQueue;
        private readonly HttpClient _client;

        public WebhookReceiverTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var payloadQueueSettings = configuration.GetSection("PayloadQueueSettings").Get<PayloadQueueSettings>();

            _payloadQueue = new PayloadQueue(payloadQueueSettings);
            _payloadQueue.Clear();



            var server = new TestServer(new WebHostBuilder().UseConfiguration(configuration).UseStartup<Startup>());
            _client = server.CreateClient();
        }

        public void Dispose()
        {
            _payloadQueue.Clear();
        }

        [Fact]
        public void Can_post_to_webhook_receiver_url()
        {
            var payloadSignatureTuple = ContactPayloadGenerator.CreateSamplePayload(1, true, 0);

            var result = SendPayload(payloadSignatureTuple);

            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal(1, _payloadQueue.Length());
        }

        [Fact]
        public void Unauthorised_if_signature_is_incorrect()
        {
            var payloadSignatureTuple = ContactPayloadGenerator.CreateSamplePayload(1, false, 0);

            var result = SendPayload(payloadSignatureTuple);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.Equal(0, _payloadQueue.Length());
        }

        public HttpResponseMessage SendPayload(PayloadSignatureTuple payloadSignatureTuple)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payloadSignatureTuple.Payload), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_client.BaseAddress}webhooks")
            {
                Content = content
            };

            request.Headers.Add("x-xero-signature", payloadSignatureTuple.Signature);

            return _client.SendAsync(request).Result;
        }
    }

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

            string signature;
            if (useValidSigningKey)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var webhookSettings = configuration.GetSection("WebhookSettings").Get<WebhookSettings>();

                signature = new SignatureVerifier(webhookSettings).GenerateSignature(payloadString);
            }
            else
            {
                var webhookSettings = new WebhookSettings
                {
                    SigningKey = "InvalidKey"
                };

                signature = new SignatureVerifier(webhookSettings).GenerateSignature(payloadString);
            }

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
