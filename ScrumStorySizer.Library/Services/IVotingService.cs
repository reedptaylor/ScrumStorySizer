﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IVotingService
    {
        public HubConnection HubConnection { get; }
        public string StoryName { get; set; }
        public List<SizeVote> StorySizeVotes { get; }
        public bool ShowVotes { get; }
        public int TimeLeft { get; }
        public event Action OnChange;
        public void AddStorySizeVotes(SizeVote vote);
        public void ClearStorySizeVotes();
        public void UpdateStoryName(string name);
        public void RevealVotes();
        public void TimeRemaining(int seconds);
        public void CancelTimer();
    }
}
