using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer("Server=tcp:evettainen.database.windows.net,1433;Initial Catalog=Silicon-SqlServer;Persist Security Info=False;User ID=SqlAdmin;Password=ZWQ3OTM1MzUtMDA3Ni00YmRhLWIyOGQtZmEzY2ZkZDkyMTcx;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

        return new DataContext(optionsBuilder.Options);
    }
}
