using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new RouteConfig
            {
                RouteId = "loxone-backend-route",
                ClusterId = "loxone-backend-cluster",
                Match = new RouteMatch
                {
                    Path = "/loxone/{**catch-all}"
                },
                Transforms = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["PathRemovePrefix"] = "/loxone"
                    }
                }
            }
        },
        new[]
        {
            new ClusterConfig
            {
                ClusterId = "loxone-backend-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    {
                        "backend1",
                        new DestinationConfig
                        {
                            Address = "http://localhost:5000/"
                        }
                    }
                }
            }
        });

var app = builder.Build();

app.MapGet("/", () => "Loxone proxy server běží.");

app.MapReverseProxy();

app.Run("http://0.0.0.0:8080");