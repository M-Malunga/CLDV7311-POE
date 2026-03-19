using Microsoft.EntityFrameworkCore;
using ST10296771_CLDV7311_POE.Models;
using System.Reflection.Emit;

namespace ST10296771_CLDV7311_POE.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingRequest> BookingRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BookingRequest>()
                .HasOne(br => br.Customer)
                .WithMany()
                .HasForeignKey(br => br.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
