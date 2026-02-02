using GateFall.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Bunu ekle
using Microsoft.EntityFrameworkCore;

namespace GateFall.Data
{
    // DbContext yerine IdentityDbContext yapıyoruz
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
    }
}