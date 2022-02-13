using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.XForwardedFor);

// Add services to the container.

var app = builder.Build();

app.UseForwardedHeaders();

app.MapPost("/api/12B4499B-4827-40C3-80F7-491C31C96973", async (HttpContext context) =>
{
    if (context.Connection.RemoteIpAddress != null)
    {
        var file = await File.ReadAllLinesAsync("/var/ipprocessor/whitelist.conf");
        List<string> list = new(file);
        foreach (string line in list)
            if (line.Contains(context.Connection.RemoteIpAddress.ToString()))
                return Results.Ok();
        list.Add("allow " + context.Connection.RemoteIpAddress.ToString() + ";");
        list.RemoveAll(x => x == "deny all;" || string.IsNullOrWhiteSpace(x));
        list.Add("deny all;");
        await File.WriteAllLinesAsync("/var/ipprocessor/whitelist.conf", list);
        return Results.Ok();
    }
    return Results.BadRequest();
});

app.MapPost("/api/840F343C-69EF-4C53-BEAD-B4AA243B9D10", async (HttpContext context) =>
{
    if (context.Connection.RemoteIpAddress != null)
    {
        var file = await File.ReadAllLinesAsync("/var/ipprocessor/blacklist.conf");
        List<string> list = new(file);
        foreach (string line in list)
            if (line.Contains(context.Connection.RemoteIpAddress.ToString()))
                return Results.Ok();
        list.Add("deny " + context.Connection.RemoteIpAddress.ToString() + ";");
        list.RemoveAll(x => x == "allow all;" || string.IsNullOrWhiteSpace(x));
        await File.WriteAllLinesAsync("/var/ipprocessor/blacklist.conf", list);
        return Results.Ok();
    }
    return Results.BadRequest();
});


app.Run();

