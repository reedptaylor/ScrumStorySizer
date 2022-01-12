using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StoryDetails : IDisposable
    {
        [Inject] IVotingService PokerVote { get; set; }

        private IEnumerable<MarkupDescriptionField> _descriptionFields;

        protected override void OnInitialized()
        {
            PokerVote.OnChange += OnUpdate;
            OnUpdate();
        }

        public void Dispose()
        {
            PokerVote.OnChange -= OnUpdate;
        }

        void OnUpdate()
        {
            _descriptionFields = PokerVote.WorkItem?.DescriptionFields?
                .ConvertAll(field => new MarkupDescriptionField(field))
                .Where(field => !string.IsNullOrWhiteSpace(field.Value));
            InvokeAsync(StateHasChanged);
        }
    }
}