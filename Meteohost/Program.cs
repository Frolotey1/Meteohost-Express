using Meteohost;
using Meteohost.Core.Impl.Messenger;
using Meteohost.Factories;
using Meteohost.Services;
using Meteohost.MiddleWare;
using MeteoLib;
using MeteoLib.AviationWeather;
using MeteoLib.AviationWeather.AwcSource;
using MeteoLib.Impl.Delivery;
using MeteoLib.Impl.MeteoAPI;
using MeteoLib.Interfaces;
using MeteoLib.LoadService;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meteohost API",
        Version = "v1.0.0",
        Description = "API для интеграции с Express мессенжером"
    });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Description = "Введите API ключ: test-api-key"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Logging.AddSimpleConsole();
builder.Services.AddControllers();

builder.Services.AddSingleton<IMeteoAPI, API>();
builder.Services.AddSingleton<IAWC, AWC>();
builder.Services.AddSingleton<IRepo, Mega10Repository>();
builder.Services.AddSingleton<IConnectionFactory, ConfigConnectionFactory>();
builder.Services.AddSingleton<IAirports, AirportsConfig>();
builder.Services.AddSingleton<AwcSourceConfig>();
builder.Services.AddSingleton(p => p.GetService<AwcSourceConfig>().GetSource());
builder.Services.AddSingleton<Delivery>();

builder.Services.AddTransient<MessengerFactory>();
builder.Services.AddTransient(p => p.GetRequiredService<MessengerFactory>().GetFltt());
builder.Services.AddTransient(p => p.GetRequiredService<MessengerFactory>().GetTelegram());

builder.Services.AddTransient<IssueTrackerFactory>();
builder.Services.AddTransient(p => p.GetRequiredService<IssueTrackerFactory>().GetTracker());

builder.Services.AddTransient<AlertFactory>();
builder.Services.AddTransient(p => p.GetRequiredService<AlertFactory>().GetAlert());

builder.Services.AddTransient<LoadService>();

builder.Services.AddSingleton<IAirportDictionary, Mega10Dict>();

builder.Services.AddScoped<IExpressService, ExpressService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meteohost API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableTryItOutByDefault();
        c.DisplayRequestDuration();
        
        c.HeadContent = @"
        <style>
            .custom-info {
                background: #1e293b;
                color: #e2e8f0;
                padding: 20px 30px;
                margin: 20px 0;
                border-radius: 8px;
                border-left: 4px solid #3b82f6;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            }
            .custom-info h1 { color: #60a5fa; margin-top: 0; font-size: 24px; }
            .custom-info h2 { color: #93c5fd; font-size: 18px; margin-top: 20px; }
            .custom-info code { background: #0f172a; padding: 2px 8px; border-radius: 4px; color: #fcd34d; font-size: 14px; }
            .custom-info .highlight { background: #0f172a; padding: 10px 15px; border-radius: 4px; border-left: 3px solid #fcd34d; margin: 5px 0; }
            .custom-info .warning { background: #451a1a; border-left: 4px solid #ef4444; padding: 10px 15px; border-radius: 4px; margin: 10px 0; }
            .custom-info .success { background: #064e3b; border-left: 4px solid #10b981; padding: 10px 15px; border-radius: 4px; margin: 10px 0; }
            .custom-info .badge { display: inline-block; background: #3b82f6; color: white; padding: 2px 10px; border-radius: 12px; font-size: 12px; font-weight: bold; }
            .custom-info .endpoint { color: #34d399; font-weight: bold; }
            .custom-info .method { color: #f472b6; font-weight: bold; }
        </style>
        <div class='custom-info'>
            <h1>Аутентификация</h1>
            
            <div class='warning'>
                <p><strong>ВАЖНО ДЛЯ SWAGGER:</strong></p>
                <p>1. Нажмите <strong>Authorize</strong> в правом верхнем углу</p>
                <p>2. Введите: <code>test-api-key</code></p>
                <p>3. Нажмите <strong>Authorize</strong></p>
                <p>4. <strong>ОБНОВИТЕ СТРАНИЦУ (F5)</strong></p>
                <p>5. Затем нажмите <strong>Try it out</strong> на любом эндпоинте</p>
            </div>
            
            <div class='success'>
                <p><strong>Проверка через curl:</strong></p>
                <div class='highlight'>
                    <code>curl -X GET ""http://localhost:5000/api/Proof/ping"" -H ""X-Api-Key: test-api-key""</code>
                </div>
            </div>
            
            <p><strong>Групповой ChatId:</strong> <code>5455c9c9-3dc6-590b-be34-74f82f46308e</code></p>
            
            <h2>Доступные эндпоинты</h2>
            <div class='highlight'>
                <p><span class='method'>POST</span> <span class='endpoint'>/api/Proof/send</span> - Отправить сообщение</p>
                <p><span class='method'>POST</span> <span class='endpoint'>/api/Proof/send-default</span> - Отправить в группу по умолчанию</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Proof/chats</span> - Список чатов</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/api/Proof/ping</span> - Проверка работы</p>
                <p><span class='method'>GET</span> <span class='endpoint'>/health</span> - Health check</p>
            </div>
        </div>
        ";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ApiKeyMiddleWare>();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();
