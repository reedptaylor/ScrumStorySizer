using Microsoft.AspNetCore.SignalR;
using ScrumStorySizer.Library;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Server.Hubs;

namespace ScrumStorySizer.Server.Services;

public class CommandService
{
    private readonly VotingServiceData _votingServiceCache;
    private readonly IHubContext<VoteHub> _hubContext;

    public CommandService(VotingServiceData votingServiceCache, IHubContext<VoteHub> hubContext) : base()
    {
        _votingServiceCache = votingServiceCache;
        _hubContext = hubContext;
    }

    public async Task AddConnectedClientAsync()
    {
        _votingServiceCache.ConnectedClients++;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _votingServiceCache.ConnectedClients);
    }

    public async Task RemoveConnectedClientAsync()
    {
        _votingServiceCache.ConnectedClients--;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _votingServiceCache.ConnectedClients);
    }

    public async Task AddStorySizeVotesAsync(SizeVote vote)
    {
        _votingServiceCache.StorySizeVotes.RemoveAll(item => item.User == vote.User);
        _votingServiceCache.StorySizeVotes.Add(vote);
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_ADD_VOTES, vote);
    }

    public async Task ClearStorySizeVotesAsync()
    {
        _votingServiceCache.StorySizeVotes.Clear();
        _votingServiceCache.ShowVotes = false;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_CLEAR_VOTES);
    }

    public async Task RevealVotesAsync()
    {
        _votingServiceCache.ShowVotes = true;
        _votingServiceCache.TimeLeft = 0;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_REVEAL_VOTES);
    }

    public async Task UpdateWorkItemAsync(WorkItem workItem)
    {
        _votingServiceCache.WorkItem = workItem;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, workItem);
    }

    public async Task CancelTimerAsync()
    {
        _votingServiceCache.TimeLeft = 0;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_CANCEL_TIMER);
    }

    public async Task UpdateTimeRemainingAsync(int seconds)
    {
        _votingServiceCache.TimeLeft = seconds;
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_TIME_REMAINING, seconds);
    }
}