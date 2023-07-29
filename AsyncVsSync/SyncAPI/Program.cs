var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    var dbConnector = new DbConnector();
    var apiConnector = new ApiConnector();

    var dbResult = dbConnector.Get();
    var apiResult = apiConnector.Get();

    return dbResult.Value + apiResult.Value;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class DbConnector
{
    public Result Get()
    {
        Thread.Sleep(500);
        return new() { Value = 12 };
    }
}

class ApiConnector
{
    public Result Get()
    {
        Thread.Sleep(500);
        return new() { Value = 13 };
    }
}

class Result
{
    public int Value { get; set; }
}