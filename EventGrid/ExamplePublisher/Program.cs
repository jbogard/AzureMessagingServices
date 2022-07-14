using Azure.Core;
using Azure.Identity;
using Azure.Messaging.EventGrid;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var options = new DefaultAzureCredentialOptions
{
    VisualStudioTenantId = builder.Configuration["TenantId"],
};
builder.Services.AddSingleton<TokenCredential>(new DefaultAzureCredential(options));

var endpoint = new Uri(builder.Configuration["TopicUrl"]);

builder.Services.AddTransient(sp => new EventGridPublisherClient(endpoint, sp.GetRequiredService<TokenCredential>()));

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
