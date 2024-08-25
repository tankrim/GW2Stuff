﻿using BarFoo.Data.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BarFoo.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BarFooDbContext>
{
    public const string AppName = "BarFoo";
    public const string DbName = "barfoo.db";

    public DesignTimeDbContextFactory() { }

    public BarFooDbContext CreateDbContext(string[] args)
    {
        var appDataPath = GetAppDataPath(AppName);
        var dbPath = Path.Combine(appDataPath, DbName);

        var connectionString = $"Data Source={dbPath}";

        var optionsBuilder = new DbContextOptionsBuilder<BarFooDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new BarFooDbContext(optionsBuilder.Options);
    }

    private static string GetAppDataPath(string appName)
    {
        ArgumentException.ThrowIfNullOrEmpty(appName, nameof(appName));

        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string fullPath = Path.Combine(appDataPath, appName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }
}
