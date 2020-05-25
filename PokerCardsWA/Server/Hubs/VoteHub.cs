using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PokerCardsShared.Models;

namespace PokerCardsWA.Server.Hubs
{
    public class VoteHub : Hub
    {
        private readonly CacheService _cacheService;

        public VoteHub(CacheService cacheService) : base()
        {
            _cacheService = cacheService;
        }

        public async Task AddStorySizeVotes(SizeVote vote)
        {
            await Clients.All.SendAsync("ReceiveAddStorySizeVotes", vote);
            _cacheService.StorySizeVotes.RemoveAll(item => item.User == vote.User);
            _cacheService.StorySizeVotes.Add(vote);
        }

        public async Task ClearStorySizeVotes()
        {
            await Clients.All.SendAsync("ReceiveClearStorySizeVotes");
            _cacheService.StorySizeVotes.Clear();
            _cacheService.ShowVotes = false;
        }

        public async Task RevealVotes()
        {
            _cacheService.ShowVotes = true;
            await Clients.All.SendAsync("ReceiveRevealVotes");
        }

        public async Task UpdateStoryName(string name)
        {
            _cacheService.StoryName = name;
            await Clients.All.SendAsync("ReceiveUpdateStoryName", name);
        }

        public async Task NewConnection()
        {
            await Clients.Caller.SendAsync("ReceiveCache", _cacheService.StoryName, _cacheService.StorySizeVotes, _cacheService.ShowVotes);
        }
    }
}
