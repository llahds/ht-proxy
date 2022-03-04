using RestSharp;
using System.Net;
using System.Net.Sockets;

namespace HT.NATProxy
{
    public class Client : IDisposable
    {
        private readonly Socket socket;
        private readonly string ip;
        private readonly int port;
        private readonly string method;
        private readonly string url;
        private readonly string localHost;
        private readonly string localPath;
        private readonly CancellationTokenSource cts;

        public Client(string ip, int port, string remoteMethod, string remoteUrl, string localHost, string localPath)
        {
            this.cts = new CancellationTokenSource();
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.ip = ip;
            this.port = port;
            this.method = remoteMethod;
            this.url = remoteUrl;
            this.localHost = localHost;
            this.localPath = localPath;
        }

        public void Start()
        {
            this.socket.Connect(IPAddress.Parse(this.ip), this.port);

            var ok = this.socket.ReceiveMessage<Ok>(this.cts.Token);
            
            this.socket.SendMessage(new Add { Method = method, Url = url });

            Task.Run(async () =>
            {
                while (this.cts.IsCancellationRequested == false && this.socket.Connected)
                {
                    var request = await this.socket.ReceiveMessage<Request>(this.cts.Token);

                    var response = new Response { };

                    if (request?.Method == "GET")
                    {
                        response = await this.Get(request);
                    }
                    else if (request?.Method == "POST")
                    {
                        response = await this.Post(request);
                    }

                    this.socket.SendMessage(response);
                }
            });
        }

        private async Task<Response> Get(Request request)
        {
            try
            {
                var client = new RestClient(this.localHost);

                var rr = new RestRequest(this.localPath + request.QueryString, Method.Get);

                //rr.AddHeader("Accept", request.ContentType);

                var r = await client.ExecuteAsync(rr);

                return new Response
                {
                    ContentType = r.ContentType,
                    Content = r.Content,
                    StatusCode = (int)r.StatusCode
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return new Response();
        }

        private async Task<Response> Post(Request request)
        {
            try
            {
                var client = new RestClient(this.localHost);

                var rr = new RestRequest(this.localPath + request.QueryString, Method.Post);

                rr.AddStringBody(request.Payload, request.ContentType);

                var r = await client.ExecuteAsync(rr);

                return new Response
                {
                    ContentType = r.ContentType,
                    Content = r.Content,
                    StatusCode = (int)r.StatusCode
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return new Response();
        }

        public void Stop()
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.socket.Close();
        }

        public void Dispose()
        {
            this.cts.Cancel();
            this.socket.Dispose();
        }
    }
}
