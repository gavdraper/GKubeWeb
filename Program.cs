Globals.StartupTime = DateTime.Now;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Logger.LogInformation("The app started");

app.MapGet("/", () => "Hello World From " + System.Environment.MachineName) ;

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
    var count = int.Parse(request.Query["count"]);
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

// List Env Vars


app.Run();
