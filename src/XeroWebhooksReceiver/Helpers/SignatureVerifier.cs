using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XeroWebhooksReceiver.Helpers
{
    public class SignatureVerifier : ISignatureVerifier
    {
        public bool VerifySignature(string payload, string signatureHeader)
        {
            var generatedSignature = GenerateSignature(File.ReadAllText("Resources/SigningKey.txt"), payload);

            return generatedSignature == signatureHeader;
        }

        public string GenerateSignature(string signingKey, string dataToHash)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey)))
            {
                var messageBytes = Encoding.UTF8.GetBytes(dataToHash);
                var hash = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
