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
        public DbSet<BookingDetailsView> BookingDetailsViews { get; set; }

        // NEW: EventType DbSet
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Booking unique constraint for double booking prevention
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique()
                .HasDatabaseName("IX_Booking_Venue_Date");

            // BookingRequest relationships
            modelBuilder.Entity<BookingRequest>()
                .HasOne(br => br.Customer)
                .WithMany()
                .HasForeignKey(br => br.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Table mappings
            modelBuilder.Entity<Venue>().ToTable("Venue");
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Booking>().ToTable("Booking");
            modelBuilder.Entity<BookingRequest>().ToTable("BookingRequest");
            modelBuilder.Entity<Employee>().ToTable("Employee");
            modelBuilder.Entity<EventType>().ToTable("EventTypes");

            // ============================================
            // NEW: EventType Configuration
            // ============================================
            modelBuilder.Entity<EventType>(entity =>
            {
                entity.HasKey(e => e.EventTypeId);
                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Description)
                    .HasMaxLength(200);
                entity.Property(e => e.IconClass)
                    .HasMaxLength(50)
                    .HasDefaultValue("bi-calendar-event");
                entity.Property(e => e.DisplayOrder)
                    .HasDefaultValue(0);
                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Index for better performance
                entity.HasIndex(e => e.CategoryName)
                    .IsUnique()
                    .HasDatabaseName("IX_EventType_CategoryName");
                entity.HasIndex(e => e.DisplayOrder)
                    .HasDatabaseName("IX_EventType_DisplayOrder");
            });

            // ============================================
            // NEW: Event Configuration with EventType
            // ============================================
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event");

                // Relationship with EventType
                entity.HasOne(e => e.EventType)
                    .WithMany(et => et.Events)
                    .HasForeignKey(e => e.EventTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Properties configuration
                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.TicketPrice)
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageFileName)
                    .HasMaxLength(255);
                entity.Property(e => e.ImageContentType)
                    .HasMaxLength(100);

                // Indexes for better query performance
                entity.HasIndex(e => e.EventDate)
                    .HasDatabaseName("IX_Event_EventDate");
                entity.HasIndex(e => e.EventTypeId)
                    .HasDatabaseName("IX_Event_EventTypeId");
                entity.HasIndex(e => new { e.EventDate, e.IsPublic })
                    .HasDatabaseName("IX_Event_Date_Public");
            });

            // ============================================
            // NEW: Venue Configuration with Availability Fields
            // ============================================
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.ToTable("Venue");

                // Properties configuration
                entity.Property(v => v.VenueName)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(v => v.Location)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(v => v.ContactPhone)
                    .HasMaxLength(20);
                entity.Property(v => v.ContactEmail)
                    .HasMaxLength(100);
                entity.Property(v => v.ImageFileName)
                    .HasMaxLength(255);
                entity.Property(v => v.ImageContentType)
                    .HasMaxLength(100);
                entity.Property(v => v.OperatingHours)
                    .HasMaxLength(100)
                    .HasDefaultValue("9:00 AM - 9:00 PM");
                entity.Property(v => v.DaysAvailable)
                    .HasMaxLength(200)
                    .HasDefaultValue("Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday");
                entity.Property(v => v.Amenities)
                    .HasMaxLength(500);
                entity.Property(v => v.IsAvailable)
                    .HasDefaultValue(true);
                entity.Property(v => v.IsIndoor)
                    .HasDefaultValue(true);
                entity.Property(v => v.HasParking)
                    .HasDefaultValue(true);
                entity.Property(v => v.IsWheelchairAccessible)
                    .HasDefaultValue(true);

                // Indexes for filtering
                entity.HasIndex(v => v.IsAvailable)
                    .HasDatabaseName("IX_Venue_IsAvailable");
                entity.HasIndex(v => v.Location)
                    .HasDatabaseName("IX_Venue_Location");
                entity.HasIndex(v => new { v.IsAvailable, v.Capacity })
                    .HasDatabaseName("IX_Venue_Available_Capacity");
            });

            // ============================================
            // NEW: Booking Details View Configuration (Read-only)
            // ============================================
            modelBuilder.Entity<BookingDetailsView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("View_BookingDetails");
                entity.Property(b => b.BookingId).HasColumnName("BookingId");
                entity.Property(b => b.EventName).HasColumnName("EventName");
                entity.Property(b => b.VenueName).HasColumnName("VenueName");
                entity.Property(b => b.CustomerName).HasColumnName("CustomerName");
                entity.Property(b => b.BookingStatus).HasColumnName("BookingStatus");
            });

            // ============================================
            // Seed Data for EventTypes
            // ============================================
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, CategoryName = "Conference", Description = "Professional conferences, seminars, and workshops", IconClass = "bi-people-fill", DefaultCapacity = 200, DisplayOrder = 1, IsActive = true },
                new EventType { EventTypeId = 2, CategoryName = "Wedding", Description = "Wedding ceremonies and receptions", IconClass = "bi-suit-heart-fill", DefaultCapacity = 150, DisplayOrder = 2, IsActive = true },
                new EventType { EventTypeId = 3, CategoryName = "Concert", Description = "Music concerts and live performances", IconClass = "bi-music-note-beamed", DefaultCapacity = 500, DisplayOrder = 3, IsActive = true },
                new EventType { EventTypeId = 4, CategoryName = "Corporate", Description = "Corporate meetings, product launches, and galas", IconClass = "bi-briefcase-fill", DefaultCapacity = 100, DisplayOrder = 4, IsActive = true },
                new EventType { EventTypeId = 5, CategoryName = "Private Party", Description = "Birthday parties, anniversaries, and private celebrations", IconClass = "bi-gift-fill", DefaultCapacity = 80, DisplayOrder = 5, IsActive = true },
                new EventType { EventTypeId = 6, CategoryName = "Exhibition", Description = "Art exhibitions, trade shows, and expos", IconClass = "bi-palette-fill", DefaultCapacity = 300, DisplayOrder = 6, IsActive = true },
                new EventType { EventTypeId = 7, CategoryName = "Sports", Description = "Sports events and tournaments", IconClass = "bi-trophy-fill", DefaultCapacity = 400, DisplayOrder = 7, IsActive = true },
                new EventType { EventTypeId = 8, CategoryName = "Workshop", Description = "Training sessions and workshops", IconClass = "bi-lightbulb-fill", DefaultCapacity = 50, DisplayOrder = 8, IsActive = true },
                new EventType { EventTypeId = 9, CategoryName = "Charity", Description = "Charity events and fundraisers", IconClass = "bi-heart-fill", DefaultCapacity = 120, DisplayOrder = 9, IsActive = true },
                new EventType { EventTypeId = 10, CategoryName = "Networking", Description = "Networking events and meetups", IconClass = "bi-chat-dots-fill", DefaultCapacity = 60, DisplayOrder = 10, IsActive = true }
            );
        }
    }
}