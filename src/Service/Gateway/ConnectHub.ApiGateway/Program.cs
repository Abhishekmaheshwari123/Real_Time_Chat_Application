using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                    "http://localhost:5173",
                    "http://127.0.0.1:5173",
                    "http://localhost:7000",
                    "http://127.0.0.1:7000",
                    "http://localhost:3000",
                    "http://127.0.0.1:3000",
                    "http://localhost:5500",
                    "http://127.0.0.1:5500",
                    "http://localhost:5501",
                    "http://127.0.0.1:5501",
                    "https://deft-blancmange-e4b2a1.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var authBaseUrl = builder.Configuration["Upstreams:Auth"] ?? "http://localhost:5221";
var chatBaseUrl = builder.Configuration["Upstreams:Chat"] ?? "http://localhost:5262";
var mediaBaseUrl = builder.Configuration["Upstreams:Media"] ?? "http://localhost:5264";
var notificationBaseUrl = builder.Configuration["Upstreams:Notification"] ?? "http://localhost:5226";

var routes = new[]
{
    new RouteConfig
    {
        RouteId = "auth-route",
        ClusterId = "auth-cluster",
        Match = new RouteMatch { Path = "/api/auth/{**catch-all}" }
    },
    new RouteConfig
    {
        RouteId = "chat-route",
        ClusterId = "chat-cluster",
        Match = new RouteMatch { Path = "/api/chat/{**catch-all}" }
    },
    new RouteConfig
    {
        RouteId = "media-route",
        ClusterId = "media-cluster",
        Match = new RouteMatch { Path = "/api/media/{**catch-all}" }
    },
    new RouteConfig
    {
        RouteId = "notification-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/api/notification/{**catch-all}" }
    },
    new RouteConfig
    {
        RouteId = "notification-root-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/api/notification" }
    },
    new RouteConfig
    {
        RouteId = "notification-command-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/generate-token" }
    },
    new RouteConfig
    {
        RouteId = "notification-post-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/notifications" }
    },
    new RouteConfig
    {
        RouteId = "chat-hub-route",
        ClusterId = "chat-cluster",
        Match = new RouteMatch { Path = "/chatHub/{**catch-all}" }
    },
    new RouteConfig
    {
        RouteId = "chat-hub-root-route",
        ClusterId = "chat-cluster",
        Match = new RouteMatch { Path = "/chatHub" }
    },
    new RouteConfig
    {
        RouteId = "notification-hub-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/notificationHub/{**catch-all}" }
    }
    ,
    new RouteConfig
    {
        RouteId = "notification-hub-root-route",
        ClusterId = "notification-cluster",
        Match = new RouteMatch { Path = "/notificationHub" }
    }
};

var clusters = new[]
{
    new ClusterConfig
    {
        ClusterId = "auth-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["auth"] = new() { Address = authBaseUrl }
        }
    },
    new ClusterConfig
    {
        ClusterId = "chat-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["chat"] = new() { Address = chatBaseUrl }
        }
    },
    new ClusterConfig
    {
        ClusterId = "media-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["media"] = new() { Address = mediaBaseUrl }
        }
    },
    new ClusterConfig
    {
        ClusterId = "notification-cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            ["notification"] = new() { Address = notificationBaseUrl }
        }
    }
};

builder.Services.AddReverseProxy()
    .LoadFromMemory(routes, clusters);

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapGet("/", () => Results.Ok(new
{
    service = "ConnectHub API Gateway",
    status = "running"
}));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapReverseProxy();

app.Run();