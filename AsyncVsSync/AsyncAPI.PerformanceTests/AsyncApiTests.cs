using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace AsyncAPI.PerformanceTests;

public class AsyncApiTests
{
    public AsyncApiTests()
    {
        
    }
    
    [Fact]
    public void Get_PerformanceTests()
    {
        const string url = "http://localhost:5267";
        var getSyncApiStep = Step.Create("get", HttpClientFactory.Create(), async context =>
        {
            try
            {
                var req = Http.CreateRequest("GET", url);
                return await Http.Send(req, context);
            }
            catch
            {
                return Response.Fail();
            }
        });

        var scenario = ScenarioBuilder.CreateScenario("Async api fetch", getSyncApiStep)
            .WithWarmUpDuration(TimeSpan.FromSeconds(5))
            .WithLoadSimulations(LoadSimulation.NewKeepConstant(100, TimeSpan.FromSeconds(5)));

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
    }
}
