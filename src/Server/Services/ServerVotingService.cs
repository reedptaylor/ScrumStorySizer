using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Server.Services;

public class ServerVotingService : IVotingService, IDisposable
{
    private readonly CommandService _commandService;
    private VotingServiceData _votingServiceCache;

    public ServerVotingService(CommandService commandService, VotingServiceData votingServiceCache)
    {
        _commandService = commandService;
        _votingServiceCache = votingServiceCache;

        _commandService.OnChange += NotifyDataChanged;
    }

    public HubConnection HubConnection => null;

    public VotingServiceData VotingServiceData
    {
        get => _votingServiceCache;
        set => _votingServiceCache = value;
    }

    public event Action OnChange;

    public void AddStorySizeVotes(SizeVote vote)
    {
        _ = _commandService.AddStorySizeVotesAsync(vote);
    }

    public void CancelTimer()
    {
        _ = _commandService.CancelTimerAsync();
    }

    public void ClearStorySizeVotes()
    {
        Task.Run(async () =>
        {
            await _commandService.ClearStorySizeVotesAsync();
        });
    }

    public void RevealVotes()
    {
        _ = _commandService.RevealVotesAsync();
    }

    public void StartTimer()
    {
        _ = _commandService.StartTimer();
    }

    public void UpdateWorkItem(WorkItem workItem)
    {
        if (workItem is null) workItem = new();
        workItem.TruncateObject(); // Truncate the object if it will be to big to send over SignalR

        _ = _commandService.UpdateWorkItemAsync(workItem);
    }

    private void NotifyDataChanged()
    {
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _commandService.OnChange -= NotifyDataChanged;
    }
}