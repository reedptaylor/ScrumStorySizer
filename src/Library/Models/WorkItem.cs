namespace ScrumStorySizer.Library.Models
{
    public class WorkItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description
        {
            get => _description;
            set
            {
                _description = value.SanitizeHTML();
            }
        }

        public string AcceptanceCriteria
        {
            get => _acceptanceCriteria;
            set
            {
                _acceptanceCriteria = value.SanitizeHTML();
            }
        }

        public IEnumerable<string> Tags { get; set; }

        public void TruncateObject()
        {
            int remainingSize = Constants.MAX_SIGNALR_SIZE;

            Title = Title.LimitByteLength(remainingSize); // Strings ordered in order of importance
            _description = _description.LimitByteLength(remainingSize);
            _acceptanceCriteria = _acceptanceCriteria.LimitByteLength(remainingSize);
        }

        private string _description;
        private string _acceptanceCriteria;
    }
}

