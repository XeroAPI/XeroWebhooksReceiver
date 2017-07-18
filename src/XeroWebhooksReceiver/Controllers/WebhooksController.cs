using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XeroWebhooksReceiver.Helpers;
using XeroWebhooksReceiver.Queue;

namespace XeroWebhooksReceiver.Controllers
{
    [Route("webhooks")]
    public class WebhooksController : Controller
    {
        private readonly IQueue<string> _payloadQueue;
        private readonly ISignatureVerifier _signatureVerifier;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IQueue<string> payloadQueue, ISignatureVerifier signatureVerifier, ILogger<WebhooksController> logger)
        {
            _payloadQueue = payloadQueue;
            _signatureVerifier = signatureVerifier;
            _logger = logger;
        }

        /// <summary>
        /// Receives a webhook payload from Xero.
        /// </summary>
        /// <returns>HTTP Status 200 OK if the payload is correctly signed, 
        /// or HTTP Status 401 Unauthorised if the signature is invalid.</returns>
        [HttpPost]
        public IActionResult Post()
        {
            var payload = ReadPayload();
            _logger.LogInformation("Received a webhook");

            var signatureHeader = Request.Headers["x-xero-signature"].FirstOrDefault();

            _logger.LogInformation("Verifying the signature to confirm that the payload is from Xero.");
            var isValid = _signatureVerifier.VerifySignature(payload, signatureHeader);

            if (!isValid)
            {
                _logger.LogWarning("The signature is incorrect. Responding with HTTP 401 Unauthorised.");
                return Unauthorized();
            }

            _logger.LogInformation("The signature is correct. Saving the payload for processing later (so that we can respond as quickly as possible).");
            _payloadQueue.Enqueue(payload);

            _logger.LogInformation("Webhook is saved. Returning an empty 200 OK");
            return Ok();
        }

        private string ReadPayload()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
