using ibhayiPharmacy.Areas.Identity.Data;
using ibhayiPharmacy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ibhayiPharmacy.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<ActiveIngredient> ActiveIngredients { get; set; }
        public DbSet<DosageForm> DosageForms { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<MedicationActiveIngredient> MedicationActiveIngredient { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== PRESCRIPTION RELATIONSHIPS ====================
            // Fix the multiple relationships to ApplicationUser
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.UploadedBy)
                .WithMany()
                .HasForeignKey(p => p.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.ProcessedBy)
                .WithMany()
                .HasForeignKey(p => p.ProcessedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.DispensedBy)
                .WithMany()
                .HasForeignKey(p => p.DispensedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription to Doctor relationship
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==================== PRESCRIPTION ITEMS ====================
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Medication)
                .WithMany()
                .HasForeignKey(pi => pi.MedicationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==================== MEDICATION RELATIONSHIPS ====================
            modelBuilder.Entity<Medication>()
                .HasOne(m => m.DosageForm)
                .WithMany()
                .HasForeignKey(m => m.DosageFormId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Medication>()
                .HasOne(m => m.Supplier)
                .WithMany()
                .HasForeignKey(m => m.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==================== MANY-TO-MANY: MEDICATION <-> ACTIVE INGREDIENTS ====================
            modelBuilder.Entity<MedicationActiveIngredient>()
                .HasKey(mai => new { mai.MedicationId, mai.ActiveIngredientId });

            modelBuilder.Entity<MedicationActiveIngredient>()
                .HasOne(mai => mai.Medication)
                .WithMany(m => m.MedicationActiveIngredients)
                .HasForeignKey(mai => mai.MedicationId);

            modelBuilder.Entity<MedicationActiveIngredient>()
                .HasOne(mai => mai.ActiveIngredient)
                .WithMany(ai => ai.MedicationActiveIngredients)
                .HasForeignKey(mai => mai.ActiveIngredientId);

            // ==================== INDEXES ====================
            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.UploadDate);

            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.PrescriptionDate);

            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.ReadyForCollection);

            modelBuilder.Entity<Prescription>()
                .HasIndex(p => p.IsDispensed);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.IdNumber)
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.RegistrationNumber)
                .IsUnique();

            modelBuilder.Entity<Medication>()
                .HasIndex(m => m.Name)
                .IsUnique();

            // ==================== DEFAULT VALUES ====================
            modelBuilder.Entity<Prescription>()
                .Property(p => p.Status)
                .HasDefaultValue("Pending Processing");

            modelBuilder.Entity<Prescription>()
                .Property(p => p.IsDispensed)
                .HasDefaultValue(false);

            modelBuilder.Entity<Prescription>()
                .Property(p => p.ReadyForCollection)
                .HasDefaultValue(false);

            modelBuilder.Entity<Prescription>()
                .Property(p => p.IsCollected)
                .HasDefaultValue(false);

            modelBuilder.Entity<Prescription>()
                .Property(p => p.CustomerNotified)
                .HasDefaultValue(false);

            modelBuilder.Entity<Prescription>()
                .Property(p => p.TotalAmount)
                .HasDefaultValue(0m);

            modelBuilder.Entity<PrescriptionItem>()
                .Property(pi => pi.RepeatsRemaining)
                .HasDefaultValue(0);

            modelBuilder.Entity<PrescriptionItem>()
                .Property(pi => pi.UnitPrice)
                .HasDefaultValue(0m);
        }
    }
}