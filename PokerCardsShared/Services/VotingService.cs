using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using PokerCardsShared.Models;

namespace PokerCardsShared.Services
{
    public class VotingService : IVotingService
    {
        public HubConnection HubConnection => null;

        public string StoryName { get; set; }

        public List<SizeVote> StorySizeVotes { get; private set; } = new List<SizeVote>();

        public bool ShowVotes { get; private set; }

        public event Action OnChange;

        public void AddStorySizeVotes(SizeVote vote)
        {
            StorySizeVotes.RemoveAll(item => item.User == vote.User);
            StorySizeVotes.Add(vote);
            NotifyDataChanged();
        }

        public void ClearStorySizeVotes()
        {
            StorySizeVotes.Clear();
            ShowVotes = false;
            NotifyDataChanged();
        }

        public void UpdateStoryName(string name)
        {
            StoryName = name;
            NotifyDataChanged();
        }

        public void RevealVotes()
        {
            ShowVotes = true;
            NotifyDataChanged();
        }

        private void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
