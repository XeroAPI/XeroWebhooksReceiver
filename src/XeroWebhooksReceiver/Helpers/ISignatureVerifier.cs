namespace XeroWebhooksReceiver.Helpers
{
    public interface ISignatureVerifier
    {
        bool VerifySignature(string payload, string signatureHeader);

        string GenerateSignature(string signingKey, string dataToHash);
    }
}