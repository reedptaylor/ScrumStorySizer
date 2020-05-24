using System;
using System.Collections.Generic;
using PokerCardsShared.Models;

namespace PokerCardsShared.Services
{
    public class VotingServiceWA : IVotingService
    {
        public string StoryName { get; set; }

        public List<SizeVote> StorySizeVotes { get; private set; } = new List<SizeVote>();

        public bool ShowVotes { get; private set; }

        public event Action OnChange;

        public void AddStorySizeVotes(SizeVote vote)
        {
            throw new NotImplementedException();
        }

        public void ClearStorySizeVotes()
        {
            throw new NotImplementedException();
        }

        public void RevealVotes()
        {
            throw new NotImplementedException();
        }

        public void UpdateStoryName(string name)
        {
            throw new NotImplementedException();
        }

        private void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
