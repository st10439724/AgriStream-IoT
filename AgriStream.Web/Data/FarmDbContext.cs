using Microsoft.EntityFrameworkCore;
using AgriStream.Web.Models;

namespace AgriStream.Web.Data
{
    public class FarmDbContext : DbContext
    {
        // The constructor passes the configuration options to the base DbContext class
        public FarmDbContext(DbContextOptions<FarmDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
    }
}


