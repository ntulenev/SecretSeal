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
}
