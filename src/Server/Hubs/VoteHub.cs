using Microsoft.AspNetCore.SignalR;
using ScrumStorySizer.Library;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Server.Services;

namespace ScrumStorySizer.Server.Hubs
{
    public class VoteHub : Hub
    {
        private readonly CommandService _commandService;
        private readonly VotingServiceData _votingServiceCache;

        public VoteHub(CommandService commandService, VotingServiceData votingServiceCache) : base()
        {
            _commandService = commandService;
            _votingServiceCache = votingServiceCache;
        }

        public async override Task OnConnectedAsync()
        {
            await _commandService.AddConnectedClientAsync();
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            await _commandService.RemoveConnectedClientAsync();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task AddStorySizeVotes(SizeVote vote)
        {
            await _commandService.AddStorySizeVotesAsync(vote);
        }

        public async Task ClearStorySizeVotes()
        {
            await _commandService.ClearStorySizeVotesAsync();
        }

        public async Task RevealVotes()
        {
            await _commandService.RevealVotesAsync();
        }

        public async Task UpdateWorkItem(WorkItem workItem)
        {
            await _commandService.UpdateWorkItemAsync(workItem);
        }

        public async Task NewConnection()
        {
            await Clients.Caller.SendAsync(Constants.HUB_COMMAND_NEW_CONNECTION, _votingServiceCache);
        }

        public async Task CancelTimer()
        {
            await _commandService.CancelTimerAsync();
        }

        public async Task TimeRemaining(int seconds)
        {
            await _commandService.UpdateTimeRemainingAsync(seconds);
        }
    }
}
