// =============================================================================
//  VISMA: Visitor Management System. Application entry point & composition root.
//  This is the ONLY place where interfaces are bound to implementations
//  (Dependency Injection). Controllers ask for interfaces; they never `new` up
//  repositories or services themselves.
// =============================================================================
using Visma.Core.Services; // service interfaces + implementations (Persons 2 & 3)
using Visma.Data;
using Visma.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---- Presentation layer ------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<NavigationHelper>();

// ---- Cross-cutting -----------------------------------------------------------
// A single clock for the whole app. Services must use TimeProvider instead of
// DateTime.Now so unit tests can substitute a fake, frozen clock.
builder.Services.AddSingleton(TimeProvider.System);

// ---- Data layer (Person 1) ---------------------------------------------------
// SQLite file lives next to the web project (src/Visma.Web/visma.db). It is
// git-ignored and recreated + seeded automatically on first run.
var dbFile = builder.Configuration["Database:FileName"] ?? "visma.db";
var dbPath = Path.Combine(builder.Environment.ContentRootPath, dbFile);
builder.Services.AddVismaData($"Data Source={dbPath}");

// ---- Business layer ----------------------------------------------------------
// Person 2 (Increment 1): uncomment once the classes exist in Visma.Core/Services
// builder.Services.AddScoped<IVisitorService, VisitorService>();
// builder.Services.AddScoped<IHostService, HostService>();
//
// Person 3 (Increment 2):
builder.Services.AddScoped<IVisitService, VisitService>();

var app = builder.Build();

// Create/upgrade the database schema and insert demo data if empty.
await app.Services.InitializeVismaDatabaseAsync();

// ---- HTTP pipeline -----------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
