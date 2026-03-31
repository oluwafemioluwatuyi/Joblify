using Joblify.Infrastructure.Extensions;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.RegisterModules(builder.Configuration, typeof(Program).Assembly);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();

