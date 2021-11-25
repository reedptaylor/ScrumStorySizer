using System.Text;
using System.Text.Json.Serialization;

namespace ScrumStorySizer.Library.Models
{
    public class WorkItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string AcceptanceCriteria { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public WorkItem GetTrimmedObject()
        {
            int remainingSize = Constants.MAX_SIGNALR_SIZE;

            WorkItem clone = (WorkItem)this.MemberwiseClone();
            clone.Title = clone.Title.LimitByteLength(remainingSize);
            clone.Description = clone.Description.LimitByteLength(remainingSize);
            clone.AcceptanceCriteria = clone.AcceptanceCriteria.LimitByteLength(remainingSize);

            return clone;
        }
    }
}

