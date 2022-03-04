namespace HT.NATProxy
{
    public class Request
    {
        public string Method { get; set; }
        public string Resource { get; set; }
        public string QueryString { get; set; }
        public string ContentType { get; set; }
        public string Payload { get; set; }
    }
}
