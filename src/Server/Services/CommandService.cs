using Microsoft.AspNetCore.SignalR;
using ScrumStorySizer.Library;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Server.Hubs;

namespace ScrumStorySizer.Server.Services;

public class CommandService
{
    private readonly VotingServiceData _votingServiceCache;
    private readonly IHubContext<VoteHub> _hubContext;

    public event Action OnChange;
    private CancellationTokenSource _cancelTimer;

    public CommandService(VotingServiceData votingServiceCache, IHubContext<VoteHub> hubContext) : base()
    {
        _votingServiceCache = votingServiceCache;
        _hubContext = hubContext;
    }

    public async Task AddConnectedClientAsync()
    {
        _votingServiceCache.ConnectedClients++;
        NotifyDataChanged();
        await _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _votingServiceCache.ConnectedClients);
    }

    public async Task RemoveConnectedClientAsync()
    {
        _votingServiceCache.ConnectedClients--;
        NotifyDataChanged();
        await _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _votingServiceCache.ConnectedClients);
    }

    public async Task AddStorySizeVotesAsync(SizeVote vote)
    {
        _votingServiceCache.StorySizeVotes.RemoveAll(item => item.User == vote.User);
        _votingServiceCache.StorySizeVotes.Add(vote);
        NotifyDataChanged();
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
        NotifyDataChanged();
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_REVEAL_VOTES);
    }

    public async Task UpdateWorkItemAsync(WorkItem workItem)
    {
        _votingServiceCache.WorkItem = workItem;
        _votingServiceCache.TimeLeft = 0;
        NotifyDataChanged();
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, _votingServiceCache.WorkItem);
    }

    public async Task CancelTimerAsync()
    {
        _cancelTimer?.Cancel();
        _votingServiceCache.TimeLeft = 0;
        NotifyDataChanged();
        await _hubContext.Clients.All.SendAsync(Constants.HUB_COMMAND_CANCEL_TIMER);
    }

    public async Task StartTimer()
    {
        _cancelTimer?.Cancel();
        _cancelTimer = new();

        _votingServiceCache.TimeLeft = Constants.TIMER_LENGTH;
        NotifyDataChanged();
        _ = _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_TIME_REMAINING, _votingServiceCache.TimeLeft);
        await Task.Delay(1000, _cancelTimer.Token);

        _ = Task.Run(async () =>
        {
            while (_votingServiceCache.TimeLeft > 0 && !_cancelTimer.IsCancellationRequested)
            {
                _votingServiceCache.TimeLeft = _votingServiceCache.TimeLeft - 1;

                if (_votingServiceCache.TimeLeft == 0)
                {
                    await RevealVotesAsync();
                    break;
                }
                else
                {
                    NotifyDataChanged();
                    await _hubContext.Clients.All.SendAsync(Constants.HUB_UPDATE_TIME_REMAINING, _votingServiceCache.TimeLeft);
                }

                await Task.Delay(1000, _cancelTimer.Token);
            }
        }, _cancelTimer.Token);
    }

    private void NotifyDataChanged()
    {
        OnChange?.Invoke();
    }
}