using System.IO;
using System.Net;
using System.Text;

namespace Okanshi.Observers
{
    /// <summary>
    /// Define how to send metrics to splunk
    /// </summary>
    public interface IHttpPoster
    {
        /// <summary>
        /// Send metric to splunk
        /// </summary>
        /// <param name="json"></param>
        /// <returns>The result json from splunk</returns>
        string SendToSplunk(string json);
    }

    /// <summary>
    /// Define how to send metrics to splunk
    /// </summary>
    public class HttpPoster : IHttpPoster
    {
        private readonly string url;
        private readonly string header;

        /// <summary>
        /// Send metric to splunk
        /// </summary>
        /// <returns>The result json from splunk</returns>
        public HttpPoster(string host, int port, string token)
        {
            url = $"https://{host}:{port}/services/collector/event";
            header = $"Splunk {token}";
        }

        /// <summary>
        /// Send
        /// </summary>
        public string SendToSplunk(string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
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