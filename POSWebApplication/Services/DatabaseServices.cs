using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;

namespace POSinCloud.Services
{
    public class DatabaseServices
    {
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseServices()
        {

        }

        public DbContextOptions<POSWebAppDbContext> ConnectDatabase(string connection)
        {
            var defaultConnectionString = $"Server=WIN-A56617PG2SR;DataBase=FortifiedCityPOS;User Id=sa;Password=sa;Encrypt=False;MultipleActiveResultSets=True";
            var connectionString = connection ?? defaultConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(connectionString);

            return optionsBuilder.Options;
        }
    }
}
