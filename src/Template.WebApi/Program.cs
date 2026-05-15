using Template.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi(builder.Configuration);

var app = builder.Build();

app.UseWebApiPipeline();

app.Run();

public partial class Program;
