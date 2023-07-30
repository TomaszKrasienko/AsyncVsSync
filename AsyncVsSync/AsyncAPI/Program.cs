using System.Runtime.CompilerServices;

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

app.MapGet("/", async () =>
{
    var dbConnector = new DbConnector();
    var apiConnector = new ApiConnector();

    var dbTask = dbConnector.GetAsync();
    var apiTask = apiConnector.GetAsync();
    // var dbResult = dbTask.Result;
    //Antypattern - this is blocking operation, because Task has status not completed
    
    await Task.WhenAll(apiTask, dbTask);

    var dbResult = dbTask.Result;
    var apiResult = apiTask.Result;
    return dbResult.Value + apiResult.Value;
});

app.Run();
 

class DbConnector
{
    public async Task<Result> GetAsync()
    {
        await Task.Delay(500);
        return new Result { Value = 12 };
    }
    
    //IL
    public Task<Result> GetAsyncOnIL()
    {
        var stateMachine = new StateMachine()
        {
            State = -1,
            MethodBuilder = AsyncTaskMethodBuilder<Result>.Create()
        };
        
        stateMachine.MethodBuilder.Start(ref stateMachine);
        return stateMachine.MethodBuilder.Task;
    }
    
    private struct  StateMachine : IAsyncStateMachine
    {
        public int State;
        public AsyncTaskMethodBuilder<Result> MethodBuilder;
        private TaskAwaiter _taskAwaiter;
        
        public void MoveNext()
        {
            //depends for quantity of await commands in methods
            try
            {
                if (State == -1)
                {
                    Console.WriteLine("Start processing DB request ...");
                    _taskAwaiter = Task.Delay(30_000).GetAwaiter();
                    //_taskAwaiter.GetResult(); // blocking
                
                    if (_taskAwaiter.IsCompleted) // happy path
                    {
                        State = 0;
                    }
                    else
                    {
                        State = 0;
                        MethodBuilder.AwaitUnsafeOnCompleted(ref _taskAwaiter, ref this);
                        return;
                    }
                }

                if (State == 0) // task completion
                {
                    _taskAwaiter.GetResult(); // non blocking
                    Console.WriteLine("Finished processing DB request ...");
                    MethodBuilder.SetResult( new Result { Value = 12 });
                    return;
                }
            }
            catch (Exception e)
            {
                MethodBuilder.SetException(e);
                State = -2;
            }      
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            //Tworzy asocjację pomiędzy method builder a maszyną stanów w
            MethodBuilder.SetStateMachine(stateMachine);
        }
    }
}

class ApiConnector
{
    public async Task<Result> GetAsync()
    {
        await Task.Delay(500);
        return new() { Value = 13 };
    }
}

class Result
{
    public int Value { get; set; }
}
