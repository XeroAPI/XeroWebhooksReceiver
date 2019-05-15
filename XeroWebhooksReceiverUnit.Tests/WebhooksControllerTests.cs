using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using WebhooksDataContracts;
using XeroWebhooksReceiver.Controllers;
using XeroWebhooksReceiver.Helpers;
using XeroWebhooksReceiver.Queue;
using Xunit;

namespace XeroWebhooksReceiverUnit.Tests
{
    public class WebhooksControllerTests
    {
        private readonly Mock<IQueue<string>> _payloadQueue;
        private readonly WebhooksController _controller;
        private readonly Stream _payloadStream = new MemoryStream();
        private readonly IHeaderDictionary _headers = new HeaderDictionary();

        public WebhooksControllerTests()
        {
            _payloadQueue = new Mock<IQueue<string>>();

            var logger = new Mock<ILogger<WebhooksController>>();
            var signatureVerifier = new Mock<ISignatureVerifier>();
            signatureVerifier.Setup(a => a.VerifySignature(It.IsAny<string>(), It.Is<string>(b => b.Equals("ValidSignature")))).Returns(true);
            signatureVerifier.Setup(a => a.VerifySignature(It.IsAny<string>(), It.Is<string>(b => b.Equals("InvalidSignature")))).Returns(false);

            var requestMock = new Mock<HttpRequest>();
            requestMock.SetupGet(it => it.Body).Returns(_payloadStream);
            requestMock.SetupGet(it => it.Headers).Returns(_headers);

            var contextMock = new Mock<HttpContext>();
            contextMock.SetupGet(it => it.Request).Returns(requestMock.Object);

            _controller = new WebhooksController(_payloadQueue.Object, signatureVerifier.Object, logger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = contextMock.Object
                }
            };
        }

        [Fact]
        public void When_signature_is_valid_queue_it_and_return_ok()
        {
            var payloadString = PrepareRequest(true);

            var result = _controller.Post();

            Assert.IsType<OkResult>(result);

            _payloadQueue.Verify(a => a.Enqueue(It.Is<string>(it => it == payloadString)), Times.Once);
        }

        [Fact]
        public void When_signature_is_invalid_return_unauthorised()
        {
            PrepareRequest(false);

            var result = _controller.Post();

            Assert.IsType<UnauthorizedResult>(result);

            _payloadQueue.Verify(a => a.Enqueue(It.IsAny<string>()), Times.Never);
        }

        private string PrepareRequest(bool withValidSignature)
        {
            var payloadSignatureTuple = GetPayloadSignatureTuple(withValidSignature);
            var payload = payloadSignatureTuple.Payload;
            var payloadString = JsonConvert.SerializeObject(payload);
            var signature = payloadSignatureTuple.Signature;

            _headers.Add("x-xero-signature", signature);

            var writer = new StreamWriter(_payloadStream);
            writer.Write(payloadString);
            writer.Flush();
            _payloadStream.Position = 0;

            return payloadString;
        }

        private PayloadSignatureTuple GetPayloadSignatureTuple(bool withValidSignature)
        {
            var payload = new Payload
            {
                Events = new List<Event>(),
                FirstEventSequence = 0,
                LastEventSequence = 0
            };

            var signature = withValidSignature ? "ValidSignature" : "InvalidSignature";

            return new PayloadSignatureTuple(payload, signature);
        }
    }
}
