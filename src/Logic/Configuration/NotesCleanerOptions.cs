using System.ComponentModel.DataAnnotations;

namespace Logic.Configuration;

/// <summary>
/// Represents configuration options for removing obsolete notes.
/// </summary>
public sealed class NotesCleanerOptions
{
    /// <summary>
    /// Specifies how many days notes should be kept before deletion.
    /// </summary>
    [Range(1, int.MaxValue)]
    public required int DaysToKeep { get; init; }

    /// <summary>
    /// Specifies how often the cleanup should run.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "365.00:00:00")]
    public required TimeSpan CleanupInterval { get; init; }
}
