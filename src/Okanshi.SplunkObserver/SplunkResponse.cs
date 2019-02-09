namespace Okanshi.SplunkObservers
{
    /// <summary>
    /// You can use this class to deserialize the answer from splunk
    /// </summary>
    public class SplunkResponse
    {
        /// <summary> text </summary>
        public string Text { get; set; }
        /// <summary> code </summary>
        public int Code { get; set; }
    }
}