using Microsoft.EntityFrameworkCore;

namespace TGBot.Models
{
    public class DataContext: DbContext
    {
        public DataContext()
        {
        }

        //public DataContext()
        //{
        //    Database.EnsureCreated();
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=DMITRY;Database=Sections;Trusted_Connection=True;TrustServerCertificate=True;");
        //}

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Teacher>()
                .HasOne(s => s.Section)
                .WithMany(t => t.Teachers)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
