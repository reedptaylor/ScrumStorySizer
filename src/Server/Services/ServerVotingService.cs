using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Server.Services;

public class ServerVotingService : IVotingService
{
    private readonly CommandService _commandService;
    private VotingServiceData _votingServiceCache;

    public ServerVotingService(CommandService commandService, VotingServiceData votingServiceCache)
    {
        _commandService = commandService;
        _votingServiceCache = votingServiceCache;
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
        Task.Run(() =>
        {
            _ = _commandService.AddStorySizeVotesAsync(vote);
            NotifyDataChanged();
        });
    }

    public void CancelTimer()
    {
        Task.Run(() =>
        {
            _ = _commandService.CancelTimerAsync();
            NotifyDataChanged();
        });
    }

    public void ClearStorySizeVotes()
    {
        Task.Run(() =>
        {
            _ = _commandService.ClearStorySizeVotesAsync();
            NotifyDataChanged();
        });
    }

    public void RevealVotes()
    {
        Task.Run(() =>
        {
            _ = _commandService.RevealVotesAsync();
            NotifyDataChanged();
        });
    }

    public void TimeRemaining(int seconds)
    {
        Task.Run(() =>
        {
            _ = _commandService.UpdateTimeRemainingAsync(seconds);
            NotifyDataChanged();
        });
    }

    public void UpdateWorkItem(WorkItem workItem)
    {
        Task.Run(() =>
        {
            if (workItem is null) workItem = new();
            workItem.TruncateObject(); // Truncate the object if it will be to big to send over SignalR

            _ = _commandService.UpdateWorkItemAsync(workItem);
            NotifyDataChanged();
        });
    }

    private void NotifyDataChanged()
    {
        OnChange?.Invoke();
    }
}