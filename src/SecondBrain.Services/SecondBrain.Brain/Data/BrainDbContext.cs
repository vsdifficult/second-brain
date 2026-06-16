using Microsoft.EntityFrameworkCore;
using SecondBrain.BuildingBlocks.EFCore;
using SecondBrain.Services.BrainService.Entities;

namespace SecondBrain.Services.BrainService.Data;

public class BrainDbContext : BaseDbContext
{
    public DbSet<NoteEntity> Notes { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<NoteBookEntity> NoteBooks { get; set; }
    public DbSet<NoteTagEntity> NoteTags { get; set; }
    public DbSet<NoteLinkEntity> NoteLinks { get; set; }

    public BrainDbContext(DbContextOptions<BrainDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Note
        modelBuilder.Entity<NoteEntity>(e =>
        {
            e.HasIndex(n => n.OwnerId);

            e.HasOne(n => n.Notebook)
                .WithMany(nb => nb.Notes)
                .HasForeignKey(n => n.NotebookId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // NoteBook
        modelBuilder.Entity<NoteBookEntity>(e =>
        {
            e.HasIndex(nb => nb.OwnerId);
        });

        // Tag
        modelBuilder.Entity<TagEntity>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();
        });

        // NoteTag (join entity for M:N Note <-> Tag)
        modelBuilder.Entity<NoteTagEntity>(e =>
        {
            e.HasKey(nt => new { nt.NoteId, nt.TagId });

            e.HasOne(nt => nt.Note)
                .WithMany(n => n.NoteTags)
                .HasForeignKey(nt => nt.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(nt => nt.Tag)
                .WithMany(t => t.NoteTags)
                .HasForeignKey(nt => nt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // NoteLink (bidirectional references between notes)
        modelBuilder.Entity<NoteLinkEntity>(e =>
        {
            e.HasIndex(l => new { l.SourceNoteId, l.TargetNoteId }).IsUnique();

            e.HasOne<NoteEntity>()
                .WithMany(n => n.OutgoingLinks)
                .HasForeignKey(l => l.SourceNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<NoteEntity>()
                .WithMany(n => n.IncomingLinks)
                .HasForeignKey(l => l.TargetNoteId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        base.OnModelCreating(modelBuilder);
    }
}