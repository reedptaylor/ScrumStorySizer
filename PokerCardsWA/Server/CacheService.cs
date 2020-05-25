using System;
using System.Collections.Generic;
using PokerCardsShared.Models;

namespace PokerCardsWA.Server
{
    public class CacheService
    {
        public string StoryName { get; set; }

        public List<SizeVote> StorySizeVotes { get; set; } = new List<SizeVote>();

        public bool ShowVotes { get; set; }
    }
}
