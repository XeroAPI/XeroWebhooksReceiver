using System;
using System.IO;
using System.Text;
using XeroWebhooksReceiver.Config;

namespace XeroWebhooksReceiver.Queue
{
    public interface IQueue<T>
    {
        void Enqueue(T value);
        T Dequeue();
        T Peek();
        void Clear();
        int Length();
    }

    public class PayloadQueue : IQueue<string>
    {
        private readonly string _filePath;

        public PayloadQueue(PayloadQueueSettings settings)
        {
            _filePath = settings.PayloadQueueFilePath;
        }

        public void Enqueue(string data)
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllLines(_filePath, new[] { data }, Encoding.UTF8);
            }
            else
            {
                File.AppendAllLines(_filePath, new[] { data }, Encoding.UTF8);
            }

        }

        public int Length()
        {
            return File.Exists(_filePath) ? File.ReadAllLines(_filePath, Encoding.UTF8).Length : 0;
        }

        public void Clear()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }

        public string Dequeue()
        {
            throw new NotImplementedException();
        }


        public string Peek()
        {
            throw new NotImplementedException();
        }
    }
}