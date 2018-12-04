using System.IO;
using System.Net;
using System.Text;

namespace Okanshi.Observers
{
    public interface IHttpPoster
    {
        /// <summary>
        /// Send metric to splunk
        /// </summary>
        /// <param name="json"></param>
        /// <returns>The result json from splunk</returns>
        string SendToSplunk(string json);
    }

    public class HttpPoster : IHttpPoster
    {
        private readonly string url;
        private readonly string header;

        /// <summary>
        /// Send metric to splunk
        /// </summary>
        /// <param name="json"></param>
        /// <returns>The result json from splunk</returns>
        public HttpPoster(string host, int port, string token)
        {
            url = $"https://{host}:{port}/services/collector/event";
            header = $"Splunk {token}";
        }

        public string SendToSplunk(string json)
        {
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers.Add("Authorization", header);

            var encoding = new UTF8Encoding();
            byte[] utfBytes = encoding.GetBytes(json);
            request.ContentLength = utfBytes.Length;

            using (Stream newStream = request.GetRequestStream())
            {
                newStream.Write(utfBytes, 0, utfBytes.Length);

                WebResponse response = request.GetResponse();

                using (Stream dataStream = response.GetResponseStream())
                using (var reader = new StreamReader(dataStream))
                {
                    string serverResponse = reader.ReadToEnd();

                    return serverResponse;
                }
            }
        }
    }
}