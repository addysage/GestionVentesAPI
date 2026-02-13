using GestionVentesAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configuration CORS pour permettre les connexions MAUI
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Ajouter les contrôleurs
builder.Services.AddControllers();

// ✅ Ajouter SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ✅ Ajouter Swagger pour la documentation API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "GestionVentes API",
        Version = "v1",
        Description = "API de synchronisation temps réel pour GestionVentesApp"
    });
});

var app = builder.Build();

// ✅ Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Redirection HTTPS (important pour Render)
app.UseHttpsRedirection();

// ✅ Activer CORS
app.UseCors("AllowAll");

// ✅ Mapper les contrôleurs
app.MapControllers();

// ✅ Mapper le Hub SignalR
app.MapHub<SalesHub>("/salesHub");

// ✅ Page d'accueil pour vérifier le statut
app.MapGet("/", () => Results.Ok(new
{
    service = "GestionVentes API",
    version = "1.0.0",
    status = "🟢 En ligne",
    timestamp = DateTime.UtcNow,
    endpoints = new
    {
        signalr = "/salesHub",
        ventes = "/api/ventes",
        swagger = "/swagger"
    }
}));

// ✅ Health check endpoint pour Render
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

Console.WriteLine("🚀 GestionVentes API démarrée");
Console.WriteLine($"🌐 Listening on: {string.Join(", ", builder.Configuration.GetSection("Urls").Get<string[]>() ?? new[] { "https://localhost:5001" })}");
Console.WriteLine("📡 SignalR Hub: /salesHub");

app.Run();