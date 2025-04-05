using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CasaHeights.Models;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
