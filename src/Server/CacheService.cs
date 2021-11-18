using System;
using System.Collections.Generic;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Server
{
    public class CacheService // Server cache of application state
    {
        public WorkItem WorkItem { get; set; }

        public List<SizeVote> StorySizeVotes { get; set; } = new List<SizeVote>();

        public bool ShowVotes { get; set; }
    }
}
