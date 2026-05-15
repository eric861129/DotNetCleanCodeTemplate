using Template.Application;
using Template.Infrastructure;
using Template.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWorkerObservability(builder.Configuration);
builder.Services.AddOptions<OutboxOptions>()
    .Bind(builder.Configuration.GetSection(OutboxOptions.SectionName))
    .Validate(options => options.IsValid(), "Outbox polling interval must be positive and batch size must be between 1 and 100.")
    .ValidateOnStart();
builder.Services.AddScoped<OutboxDispatcher>();
builder.Services.AddScoped<IOutboxMessageDispatcher, LoggingOutboxMessageDispatcher>();
builder.Services.AddHostedService<OutboxDispatcherWorker>();

var host = builder.Build();
host.Run();
