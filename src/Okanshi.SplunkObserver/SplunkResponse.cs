namespace Okanshi.Observers
{
    /// <summary>
    /// You can use this class to deserialize the answer from splunk
    /// </summary>
    public class SplunkResponse
    {
        public string Text { get; set; }
        public int Code { get; set; }
    }
}