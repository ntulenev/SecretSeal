using Abstractions;

using Logic.Configuration;

using Microsoft.Extensions.Options;

namespace Logic;

/// <summary>
/// Provides functionality for removing obsolete notes from storage.
/// </summary>
public sealed class NotesCleaner : INotesCleaner
{
    /// <summary>
    /// Initializes a new instance of the NotesCleaner class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to access note storage. Cannot be null.</param>
    /// <param name="options">The options defining how long notes should be kept. Cannot be null.</param>
    public NotesCleaner(IUnitOfWork unitOfWork, IOptions<NotesCleanerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Value is null)
        {
            throw new ArgumentException("Notes cleaner options are not configured.", nameof(options));
        }

        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task RemoveObsoleteNotesAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-_options.DaysToKeep);

        _ = await _unitOfWork.Notes
            .RemoveObsoleteNotesAsync(cutoff, cancellationToken)
            .ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private readonly IUnitOfWork _unitOfWork;
    private readonly NotesCleanerOptions _options;
}
