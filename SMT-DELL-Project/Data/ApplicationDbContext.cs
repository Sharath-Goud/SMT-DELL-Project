using Microsoft.EntityFrameworkCore;
using SMT_DELL_Project.Models;

namespace SMT_DELL_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Stage> Stages { get; set; }

        public DbSet<PMActivity> PMActivities { get; set; }

        public DbSet<PMChecklist> PMChecklists { get; set; }

        public DbSet<PMReminderSetting> PMReminderSettings { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeId)
                .IsUnique();

            modelBuilder.Entity<Stage>()
                .ToTable("ProductionStages");

            modelBuilder.Entity<Stage>()
                .HasIndex(s => s.StageName)
                .IsUnique();

            modelBuilder.Entity<Stage>()
                .HasIndex(s => s.DisplayOrder)
                .IsUnique();

            modelBuilder.Entity<PMActivity>()
                .ToTable("PMActivities");

            modelBuilder.Entity<PMActivity>()
                .HasOne(a => a.Stage)
                .WithMany()
                .HasForeignKey(a => a.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PMChecklist>()
                .ToTable("PMChecklist");

            modelBuilder.Entity<PMReminderSetting>()
                .ToTable("PMReminderSettings");

        }
    }
}