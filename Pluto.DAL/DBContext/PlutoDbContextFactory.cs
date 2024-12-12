using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pluto.DAL.DBContext;

public class PlutoDbContextFactory : IDesignTimeDbContextFactory<PlutoDbContext>
{
    public PlutoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlutoDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("PlutoAI");

        optionsBuilder.UseSqlServer(connectionString);

        return new PlutoDbContext(optionsBuilder.Options);
    }
}