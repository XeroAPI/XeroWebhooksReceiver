namespace WebhooksDataContracts
{
    public class PayloadSignatureTuple
    {
        public PayloadSignatureTuple(Payload payload, string signature)
        {
            Payload = payload;
            Signature = signature;
        }

        public Payload Payload { get; set; }

        public string Signature { get; set; }
    }
}
