using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace XeroWebhooksReceiver.Queue
{
    public class PayloadQueue : IQueue<string>
    {
        private readonly string _filePath;

        public PayloadQueue()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _filePath = configuration.GetSection("ConnectionSettings")["PayloadQueueFilePath"];
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