using System.Web;

Globals.StartupTime = DateTime.Now;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Logger.LogInformation("The app started");

/* Readyness */
app.MapGet("/ready/ready", () => Results.Ok("Ready"));
app.MapGet("/ready/not-ready", () => Results.BadRequest("Not Ready"));
app.MapGet("/ready/slow-ready/{seconds?}", (int seconds) => 
    (DateTime.Now - Globals.StartupTime).TotalSeconds > seconds ? 
        Results.Ok("Ready") : Results.BadRequest("Not Ready")
);

// Liveness
app.MapGet("liveness/live", () => Results.Ok("Alive"));

app.MapPost("liveness/failCount", (HttpRequest request) =>
{
    if(!request.Query.ContainsKey("count"))
        return Results.BadRequest("Count not provided");
    var count = int.Parse(request.Query["count"].ToString() ?? "0");
    Globals.LivenessTimesToFail = count;
    return Results.Ok("I'm Alive");
});

app.MapGet("liveness/variable", () => {
    if(Globals.LivenessTimesToFail > 0)
    {
        Globals.LivenessTimesToFail--;
        app.Logger.LogError($"Liveness Failure, Remainig {Globals.LivenessTimesToFail}");
        return Results.BadRequest($"Failures Left {Globals.LivenessTimesToFail}");
    }
    return Results.Ok();
});

// List Files
app.MapGet("folder/{path?}", (string? path) => {
    app.Logger.LogError(path ?? "Null");
    var decodedPath = HttpUtility.UrlDecode(path);
    if(!string.IsNullOrEmpty(decodedPath) && !Directory.Exists(decodedPath))
        return Results.BadRequest();
    var files = Directory.GetFiles(decodedPath ?? Directory.GetCurrentDirectory());
    return Results.Ok(files);
});

// List Env Vars
app.MapGet("environment", () => {
    var vars = Environment.GetEnvironmentVariables();
    return Results.Ok(Environment.GetEnvironmentVariables());
});

//Machine Info
app.MapGet("/machine",() => Results.Ok(Environment.MachineName));


app.Run();
