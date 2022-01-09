using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ScrumStorySizer.Library;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Server.Hubs
{
    public class VoteHub : Hub
    {
        private readonly CacheService _cacheService;

        public async override Task OnConnectedAsync()
        {
            _cacheService.ConnectedClients++;
            await Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _cacheService.ConnectedClients);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            _cacheService.ConnectedClients--;
            await Clients.All.SendAsync(Constants.HUB_UPDATE_CONNECTED_CLIENTS, _cacheService.ConnectedClients);
            await base.OnDisconnectedAsync(exception);
        }

        public VoteHub(CacheService cacheService) : base()
        {
            _cacheService = cacheService;
        }

        // Methods to update server state cache and relay updates to clients
        public async Task AddStorySizeVotes(SizeVote vote)
        {
            await Clients.All.SendAsync(Constants.HUB_COMMAND_ADD_VOTES, vote);
            _cacheService.StorySizeVotes.RemoveAll(item => item.User == vote.User);
            _cacheService.StorySizeVotes.Add(vote);
        }

        public async Task ClearStorySizeVotes()
        {
            await Clients.All.SendAsync(Constants.HUB_COMMAND_CLEAR_VOTES);
            _cacheService.StorySizeVotes.Clear();
            _cacheService.ShowVotes = false;
        }

        public async Task RevealVotes()
        {
            _cacheService.ShowVotes = true;
            await Clients.All.SendAsync(Constants.HUB_COMMAND_REVEAL_VOTES);
        }

        public async Task UpdateWorkItem(WorkItem workItem)
        {
            _cacheService.WorkItem = workItem;
            await Clients.All.SendAsync(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, workItem);
        }

        public async Task NewConnection()
        {
            await Clients.Caller.SendAsync(Constants.HUB_COMMAND_NEW_CONNECTION, _cacheService.WorkItem, _cacheService.StorySizeVotes, _cacheService.ShowVotes);
        }

        public async Task CancelTimer()
        {
            await Clients.All.SendAsync(Constants.HUB_COMMAND_CANCEL_TIMER);
        }

        public async Task TimeRemaining(int seconds)
        {
            await Clients.All.SendAsync(Constants.HUB_COMMAND_TIME_REMAINING, seconds);
        }
    }
}
