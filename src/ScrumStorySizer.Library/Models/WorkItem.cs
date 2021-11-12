using System;
using System.Collections.Generic;

namespace ScrumStorySizer.Library.Models
{
    public class WorkItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string AcceptanceCriteria { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}

