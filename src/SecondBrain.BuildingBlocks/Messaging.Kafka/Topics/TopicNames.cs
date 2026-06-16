namespace SecondBrain.BuildingBlocks.Infrastructure.Messaging.Topics;

public static class TopicNames
{
    // Notes
    public const string NoteCreated = "notes.created";
    public const string NoteUpdated = "notes.updated";
    public const string NoteDeleted = "notes.deleted";
    public const string NoteArchived = "notes.archived";
    
    // Tags
    public const string TagAdded = "tags.added";
    public const string TagRemoved = "tags.removed";
    
    // Search
    public const string SearchIndexUpdated = "search.index.updated";
    
    // System
    public const string DeadLetter = "system.dead-letter";
}

public record Topic(string Name, int Partitions = 1, short ReplicationFactor = 1)
{
    public static Topic NoteCreated => new("notes.created", 3);
    public static Topic NoteUpdated => new("notes.updated", 3);
    public static Topic TagAdded => new("tags.added", 1);
}