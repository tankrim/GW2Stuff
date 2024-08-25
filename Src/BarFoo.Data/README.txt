dotnet ef migrations add InitialCreate --context BarFooDbContext --output-dir Migrations
dotnet ef database update --context BarFooDbContext --verbose
dotnet ef migrations remove --context BarFooDbContext