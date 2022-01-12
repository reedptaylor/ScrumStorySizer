namespace ScrumStorySizer.Library.Models
{
    public class WorkItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public List<DescriptionField> DescriptionFields { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public void TruncateObject()
        {
            int remainingSize = Constants.MAX_SIGNALR_SIZE;

            Title = Title.LimitByteLength(ref remainingSize); // Strings ordered in order of importance
            DescriptionFields?.ForEach(field => field.Value = field.Value.LimitByteLength(ref remainingSize));
        }
    }
}

