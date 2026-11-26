using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Visma.Core.Interfaces;
using Visma.Data.Repositories;
using Visma.Data.Seed;

namespace Visma.Data;

/// <summary>
/// One-line wiring of the data layer into the app, so Program.cs stays tidy:
/// <c>builder.Services.AddVismaData(connectionString);</c>
/// </summary>
public static class DependencyInjection
{
    /// <summary>Register the DbContext (SQLite) and all repositories (scoped = one per web request).</summary>
    public static IServiceCollection AddVismaData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<VismaDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IVisitorRepository, VisitorRepository>();
        services.AddScoped<IHostRepository, HostRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();

        return services;
    }

    /// <summary>
    /// Apply any pending migrations (creates visma.db on first run) and seed demo data.
    /// Call once at startup.
    /// </summary>
    public static async Task InitializeVismaDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VismaDbContext>();
        var clock = scope.ServiceProvider.GetService<TimeProvider>() ?? TimeProvider.System;

        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, clock);
    }
}
