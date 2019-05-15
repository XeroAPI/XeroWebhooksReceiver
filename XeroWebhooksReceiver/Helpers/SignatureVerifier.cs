using System;
using System.Security.Cryptography;
using System.Text;
using XeroWebhooksReceiver.Config;

namespace XeroWebhooksReceiver.Helpers
{
    public interface ISignatureVerifier
    {
        bool VerifySignature(string payload, string signatureHeader);

        string GenerateSignature(string dataToHash);
    }

    public class SignatureVerifier : ISignatureVerifier
    {
        private readonly WebhookSettings _settings;

        public SignatureVerifier(WebhookSettings settings)
        {
            _settings = settings;
        }

        public bool VerifySignature(string payload, string signatureHeader)
        {
            var generatedSignature = GenerateSignature(payload);

            return generatedSignature == signatureHeader;
        }

        public string GenerateSignature(string dataToHash)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.SigningKey)))
            {
                var messageBytes = Encoding.UTF8.GetBytes(dataToHash);
                var hash = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
