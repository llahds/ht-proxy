using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace HT.NATProxy
{
    public class Server : IDisposable
    {
        private readonly Socket socket;
        private readonly CancellationTokenSource cts;
        private readonly ConcurrentDictionary<string, Socket> clients;

        public Server(string ip, int port)
        {
            this.cts = new CancellationTokenSource();
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            this.socket.Listen();
            this.clients = new ConcurrentDictionary<string, Socket>();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (this.cts.IsCancellationRequested == false)
                {
                    var client = await this.socket.AcceptAsync(this.cts.Token);

                    await Task.Delay(1000);

                    client.SendMessage(new Ok());

                    var reply = await client.ReceiveMessage<Add>(this.cts.Token);

                    var key = $"{reply.Method}:{reply.Url}";

                    this.clients.AddOrUpdate(key, client, (key, socket) => client);

                    Console.WriteLine($"Added {key} for {client.RemoteEndPoint.ToString()}");
                }
            });
        }

        public async Task<Response> Post(string url, string contentType, string queryString, string content)
        {
            var key = $"POST:{url}";

            Socket client = null;

            if (this.clients.TryGetValue(key, out client) && client?.Connected == true)
            {
                client.SendMessage(new Request { Method = "POST", Resource = url, ContentType = contentType, QueryString = queryString, Payload = content });

                return await client.ReceiveMessage<Response>(this.cts.Token);
            }

            return null;
        }

        public async Task<Response> Get(string url, string contentType, string queryString)
        {
            var key = $"GET:{url}";

            Socket client = null;

            if (this.clients.TryGetValue(key, out client) && client?.Connected == true)
            {
                client.SendMessage(new Request { Method = "GET", Resource = url, ContentType = contentType, QueryString = queryString });

                return await client.ReceiveMessage<Response>(this.cts.Token);
            }

            return null;
        }

        public void Stop()
        {
            foreach (var client in clients.Values)
            {
                client.Disconnect(false);
                client.Close();
            }

            this.cts.Cancel();
            this.socket.Close();
        }

        public void Dispose()
        {
            foreach (var client in clients.Values)
            {
                client.Dispose();
            }

            this.cts.Dispose();
            this.socket.Dispose();
        }
    }
}
