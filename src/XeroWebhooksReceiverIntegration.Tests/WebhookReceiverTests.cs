using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using WebhooksDataContracts;
using XeroWebhooksReceiver;
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
            _payloadQueue = new PayloadQueue();
            _payloadQueue.Clear();

            var server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
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
}
