using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WoWItemDb.Scraper
{
    public class Scraper
    {
        public event ResponseReceivedEventHandler OnResponseRecieved;
        static string UriBase = "https://us.api.battle.net/wow/item/{0}?locale={1}&apikey={2}";
        private readonly string _locale;
        private readonly int _startingItemId;
        private readonly int _endingItemId;
        private readonly string _apiKey;
        private readonly string _outputPath;

        private bool _complete = false;

        private Queue<string> ResponseQueue;
    

        public Scraper(string outputPath, string apiKey, string locale, int startingItemId, int endingItemId)
        {
            _outputPath = outputPath;
            _apiKey = apiKey;
            _endingItemId = endingItemId;
            _startingItemId = startingItemId;
            _locale = locale;

            ResponseQueue = new Queue<string>();
        }

        private string FormatUri(int itemId)
        {
            return string.Format(UriBase, itemId, _locale, _apiKey);
        }

        public async Task BeginScraping()
        {
            var httpClient = new HttpClient();

            Thread responseWriterThread = new Thread(ProcessQueue);

            responseWriterThread.Start();

            for (int i = _startingItemId; i <= _endingItemId; i++)
            {
                HttpStatusCode status = HttpStatusCode.BadGateway;

                do
                {
                    var responseMessage = await httpClient.GetAsync(FormatUri(i));
                    var responseBody = await responseMessage.Content.ReadAsStringAsync();
                    status = responseMessage.StatusCode;
                    OnResponseRecieved?.Invoke(this, new ReponseEventArgs(i, responseBody, (int)status));

                    if (status == HttpStatusCode.OK)
                    {
                        ResponseQueue.Enqueue(responseBody);
                    }
                } while (status != HttpStatusCode.OK && status != HttpStatusCode.NotFound); //retry incase of rate limit or some other fault 

            }
            _complete = true;
        }

        public void ProcessQueue()
        {
            var file = "";
            using (var fs = File.OpenWrite(file))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                while (ResponseQueue.Any() || !_complete)
                {
                    if (!ResponseQueue.Any())
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var itemData = ResponseQueue.Dequeue();

                    writer.WriteLine(itemData);
                    writer.Flush();
                }
            }

        }
    }

    public delegate void ResponseReceivedEventHandler(object source, ReponseEventArgs e);

    public class ReponseEventArgs : EventArgs
    {
        public ReponseEventArgs(int itemId, string responseText, int responseCode)
        {
            ItemId = itemId;
            ResponseText = responseText;
            ResponseCode = responseCode;
        }

        public int ItemId { get; }
        public string ResponseText { get; set; }
        public int ResponseCode { get; set; }
    }
}
