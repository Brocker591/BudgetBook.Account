using BudgetBook.Account.Consumers;
using BudgetBook.Account.Entities;
using BudgetBook.Account.Repositories;
using BudgetBook.Account.Settings;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;
using static MassTransit.Logging.DiagnosticHeaders.Messaging;

var builder = WebApplication.CreateBuilder(args);


ServiceSettings serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
MongoDbSettings mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();


BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.String));


builder.Services.AddSingleton<IRepository<User>>(serviceProvider =>
{

    var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
    var database = mongoClient.GetDatabase(serviceSettings.ServiceName);
    return new MongoRepository<User>(database, serviceSettings.ServiceName);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));



//Einrichtung der MessageBroker
const string RabbitMQ = "RABBITMQ";
const string ServiceBus = "SERVICEBUS";

switch (serviceSettings.MessageBroker?.ToUpper())
{
    case ServiceBus:
        builder.Services.AddMassTransit(configure =>
        {

            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.UsingAzureServiceBus((context, configurator) =>
            {
                ServiceBusSettings serviceBusSettings = builder.Configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();


                configurator.Host(serviceBusSettings.ConnectionString);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                configurator.UseMessageRetry((retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5)));
            });

        });
        break;
    case RabbitMQ:
    default:
        builder.Services.AddMassTransit(configure =>
        {

            configure.AddConsumers(Assembly.GetEntryAssembly());
            configure.UsingRabbitMq((context, configurator) =>
            {

                RabbitMQSettings rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();


                configurator.Host(rabbitMQSettings.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                configurator.UseMessageRetry((retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5)));

            });

        });
        break;
}



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

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();




