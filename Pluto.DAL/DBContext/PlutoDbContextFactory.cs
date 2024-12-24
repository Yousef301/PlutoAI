using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pluto.DAL.Exceptions;

namespace Pluto.DAL.DBContext;

public class PlutoDbContextFactory : IDesignTimeDbContextFactory<PlutoDbContext>
{
    public PlutoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlutoDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("PlutoAI") ??
                               throw new InvalidConfigurationException("Connection string configuration is missing.");

        optionsBuilder.UseNpgsql(connectionString);

        return new PlutoDbContext(optionsBuilder.Options);
    }
}