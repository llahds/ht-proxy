using HT.NATProxy;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var ip = app.Configuration["proxy:localEndpoint:ip"];
var port = app.Configuration.GetValue<int>("proxy:localEndpoint:port");

var server = new Server(ip, port);
server.Start();

app.Run(async context =>
{
    var method = context.Request.Method;
    Response response = null;

    if (method == "GET")
    {
        var contentType = context.Request.ContentType ?? "";
        var path = $"{context.Request.Path}";
        var qs = context.Request.QueryString.ToString();

        response = await server.Get(context.Request.Path, contentType, qs);
    }
    else if (method == "POST")
    {
        var contentType = context.Request.ContentType ?? "";
        var path = $"{context.Request.Path}";
        var qs = context.Request.QueryString.ToString();
        var content = "";
        using (var sr = new StreamReader(context.Request.Body))
        {
            content = await sr.ReadToEndAsync();
        }
       
        response = await server.Post(path, contentType, qs, content);
    }

    if (response != null)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        await context.Response.WriteAsync(response.Content ?? "");
    }
    else
    {
        context.Response.StatusCode = 502;
    }
});

app.Run("https://localhost:3000");

server.Stop();
server.Dispose();