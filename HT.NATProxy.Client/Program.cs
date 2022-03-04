using HT.NATProxy;
using HT.NATProxy.Client;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddEnvironmentVariables();

var configuration = builder.Build();

var proxies = configuration
    .GetSection("proxies")
    .Get<Proxy[]>()
    .Select(P => new Client(P.Ip, P.Port, P.RemoteMethod, P.RemoteUrl, P.LocalHost, P.LocalPath))
    .ToList();

proxies.ForEach(proxy => proxy.Start());

Console.WriteLine("Hit any key to stop the proxy client");
Console.ReadKey();

proxies.ForEach(proxy =>
{
    proxy.Stop();
    proxy.Dispose();
});