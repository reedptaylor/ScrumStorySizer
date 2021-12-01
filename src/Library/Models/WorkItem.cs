namespace ScrumStorySizer.Library.Models
{
    public class WorkItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string AcceptanceCriteria { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public void TruncateObject()
        {
            int remainingSize = Constants.MAX_SIGNALR_SIZE;

            Title = Title.LimitByteLength(ref remainingSize); // Strings ordered in order of importance
            Description = Description.SanitizeHTML().LimitByteLength(ref remainingSize);
            AcceptanceCriteria = AcceptanceCriteria.SanitizeHTML().LimitByteLength(ref remainingSize);
        }
    }
}

