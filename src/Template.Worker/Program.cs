using Template.Application;
using Template.Infrastructure;
using Template.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.SectionName));
builder.Services.AddScoped<OutboxDispatcher>();
builder.Services.AddScoped<IOutboxMessageDispatcher, LoggingOutboxMessageDispatcher>();
builder.Services.AddHostedService<OutboxDispatcherWorker>();

var host = builder.Build();
host.Run();
