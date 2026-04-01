using Joblify.Infrastructure.Extensions;
using Microsoft.OpenApi;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Joblify API",
        Version = "v1"
    });

    // Important for v9: ensures OpenAPI 3.1.x spec
    options.SupportNonNullableReferenceTypes();
});
builder.Services.RegisterModules(builder.Configuration, typeof(Program).Assembly);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();


app.Run();

