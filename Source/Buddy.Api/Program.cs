var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<OpenAiApiClient>();
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(provider =>
{
    var apiKey = builder.Configuration.GetValue<string>("OpenAI:ApiKey")!;
    var factory = provider.GetRequiredService<ILoggerFactory>();
    return new OpenAiApiClient(apiKey, factory.CreateLogger<OpenAiApiClient>());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
