﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Server.Hubs
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

        public async Task UpdateWorkItem(WorkItem workItem)
        {
            _cacheService.WorkItem = workItem;
            await Clients.All.SendAsync("ReceiveUpdateWorkItem", workItem);
        }

        public async Task NewConnection()
        {
            await Clients.Caller.SendAsync("ReceiveCache", _cacheService.WorkItem, _cacheService.StorySizeVotes, _cacheService.ShowVotes);
        }

        public async Task CancelTimer()
        {
            await Clients.All.SendAsync("CancelTimer");
        }

        public async Task TimeRemaining(int seconds)
        {
            await Clients.All.SendAsync("TimeRemaining", seconds);
        }
    }
}