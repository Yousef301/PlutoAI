using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pluto.DAL.DBContext;

public class PlutoDbContextFactory : IDesignTimeDbContextFactory<PlutoDbContext>
{
    public PlutoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlutoDbContext>();

        var connectionString = "Server=localhost;Database=ChatBot;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new PlutoDbContext(optionsBuilder.Options);
    }
}