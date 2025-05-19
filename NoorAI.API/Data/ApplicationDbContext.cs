using Microsoft.EntityFrameworkCore;
using NoorAI.API.Models;

namespace NoorAI.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Interview> Interviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Interview>()
            .Property(i => i.ResumeContent)
            .IsRequired()
            .HasColumnType("text");

        modelBuilder.Entity<Interview>()
            .Property(i => i.JobDescription)
            .IsRequired()
            .HasColumnType("text");

        modelBuilder.Entity<Interview>()
            .Property(i => i.Transcript)
            .IsRequired()
            .HasColumnType("text");

        modelBuilder.Entity<Interview>()
            .Property(i => i.UserName)
            .IsRequired();

        modelBuilder.Entity<Interview>()
            .Property(i => i.UserEmail)
            .IsRequired();
    }
}