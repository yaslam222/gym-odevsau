using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services; // <-- E-POSTA SERVİSİ İÇİN GEREKLİ
using Microsoft.EntityFrameworkCore;
using SporSalonuProjesi.Data;
using SporSalonuProjesi.Data.Entities;
using SporSalonuProjesi.Seed;
using SporSalonuProjesi.Service;
using SporSalonuProjesi.Services;              // <-- E-POSTA SERVİSİ İÇİN GEREKLİ

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// HATA BURADAYDI: Bu satır silindi (çift çağrıyı engellemek için)
// builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// Identity sistemi (SADECE BU BLOK KALMALI)
// Bu blok hem Rolleri (IdentityRole) hem de gevşetilmiş parola kurallarını destekler
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // PDF'teki "sau" şifresinin çalışması için kuralları gevşet
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    // Hata resminde /Account/Login'e gidiyordu (YANLIŞ)
    // Biz doğrusunun /Identity/Account/Login olduğunu belirtiyoruz (DOĞRU)
    options.LoginPath = "/Identity/Account/Login";

    // Aynı sorunu yaşamamak için diğer yolları da belirtmek iyi bir pratiktir
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Identity UI için gerekli
builder.Services.AddEndpointsApiExplorer(); // <-- ADD THIS
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("HuggingFaceClient", client =>
{
    var apiKey = builder.Configuration["HuggingFace:ApiKey"];
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
});
builder.Services.AddTransient<IEmailSender, EmailSender>();
// Bu satır, projenizi %100 ÜCRETSİZ HuggingFace servisine yönlendirir
builder.Services.AddScoped<IAIRecommendationService, SimpleRecommendationService>();


var app = builder.Build();

// ----- Seed'i BURADA çağır (Logging ile) -----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("=== DATABASE INITIALIZATION STARTED ===");

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Check database connection
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation($"Can connect to database: {canConnect}");

        if (!canConnect)
        {
            logger.LogError("Cannot connect to database! Check your connection string.");
            return; // Hata varsa burada dur
        }

        // Check and apply migrations
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        logger.LogInformation($"Pending migrations count: {pendingMigrations.Count}");

        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying migrations...");
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation($"  - {migration}");
            }
            await context.Database.MigrateAsync();
            logger.LogInformation("✓ Migrations applied successfully");
        }

        // Run seed
        logger.LogInformation("Starting seed data initialization...");
        await SeedData.Initialize(services);
        logger.LogInformation("✓ Seed data initialization completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "✗ CRITICAL ERROR during database initialization");
        if (app.Environment.IsDevelopment())
        {
            throw; // Geliştirme ortamındaysan hatayı göster
        }
    }

    logger.LogInformation("=== DATABASE INITIALIZATION COMPLETED ===");
}
// ----- Seed bitti -----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages(); // Identity UI için gerekli

app.Run();

