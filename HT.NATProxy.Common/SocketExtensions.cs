using Newtonsoft.Json;
using System.Net.Sockets;

namespace HT.NATProxy
{
    public static class SocketExtensions
    {
        public static void SendMessage(this Socket socket, object value)
        {
            try
            {
                var data = JsonConvert.SerializeObject(value);

                var buffer = new List<byte>();

                buffer.AddRange(BitConverter.GetBytes(data.Length));

                buffer.AddRange(System.Text.Encoding.ASCII.GetBytes(data));

                socket.Send(buffer.ToArray());
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static async Task<T> ReceiveMessage<T>(this Socket socket, CancellationToken token)
        {
            try
            {
                var m = new MemoryStream();
                var buffer = new byte[32767];

                var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None, token);
                m.Write(buffer, 4, bytesRead - 4);

                var objectLength = BitConverter.ToInt32(buffer);

                while (m.Length < objectLength)
                {
                    bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None, token);
                    m.Write(buffer, 0, bytesRead);
                }

                return JsonConvert.DeserializeObject<T>(System.Text.Encoding.ASCII.GetString(m.GetBuffer()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return default;
        }
    }
}
