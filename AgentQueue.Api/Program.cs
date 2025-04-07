using AgentQueue.Domain.Agents;
using AgentQueue.Domain.Commons;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ScheduleManager>(provider =>
{
    IDictionary<Shift, List<IAgent>> AgentTeams = new Dictionary<Shift, List<IAgent>>()
        {
            { Shift.Morning,  new List<IAgent>{ new TeamLead(), new MidLevelAgent(), new MidLevelAgent(),new JuniorAgent() } },
            { Shift.Afternoon, new List<IAgent> { new SeniorAgent(), new MidLevelAgent(), new JuniorAgent(), new JuniorAgent() } },
            { Shift.Night, new List<IAgent> { new MidLevelAgent(), new MidLevelAgent() } },
            { Shift.Overflow, new List<IAgent> { new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent(), new JuniorAgent() } }
        };
    var scheduleManager = new ScheduleManager();
    foreach (var team in AgentTeams)
    {
        scheduleManager.CreateTeam(team.Value, team.Key);
    }

    return scheduleManager;
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
