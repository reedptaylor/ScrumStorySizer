using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerCards.Data
{
    public class VotingService
    {
        public string StoryName { get; set; }

        public List<SizeVote> StorySizeVotes { get; private set; } = new List<SizeVote>();

        public void addStorySizeVotes(SizeVote vote)
        {
            StorySizeVotes.RemoveAll(item => item.User == vote.User);
            StorySizeVotes.Add(vote);
            NotifyDataChanged();
        }

        public void clearStorySizeVotes()
        {
            StorySizeVotes.Clear();
            showVotes = false;
            NotifyDataChanged();
        }

        public void updateStoryName(string name)
        {
            StoryName = name;
            NotifyDataChanged();
        }

        public void revealVotes()
        {
            showVotes = true;
            NotifyDataChanged();
        }

        public bool showVotes { get; set; }

        public event Action OnChange;

        public void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
