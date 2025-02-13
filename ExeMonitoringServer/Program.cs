using ExeMonitoringServer.Hubs;
using ExeMonitoringServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddGrpc();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IProcessService, ProcessServiceGrpc>();
builder.Services.AddControllers();

var app = builder.Build();

//app.UseGrpcWeb();

//app.MapGrpcService<ProcessService>().EnableGrpcWeb();

// Configure the HTTP request pipeline.
//app.MapGrpcService<ProcessServiceGrpc>();
app.MapHub<ProcessHub>("/processHub");
app.MapControllers();

app.Run();
