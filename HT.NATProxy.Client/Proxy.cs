namespace HT.NATProxy.Client
{
    public  class Proxy
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public string RemoteMethod { get; set; } 
        public string RemoteUrl { get; set; } 
        public string LocalHost { get; set; }
        public string LocalPath { get; set; }
    }
}
