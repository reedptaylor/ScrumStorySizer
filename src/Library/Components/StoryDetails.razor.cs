using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StoryDetails : IDisposable
    {
        [Inject] IVotingService VotingService { get; set; }

        private IEnumerable<MarkupDescriptionField> _descriptionFields;

        protected override void OnInitialized()
        {
            VotingService.OnChange += OnUpdate;
            OnUpdate();
        }

        public void Dispose()
        {
            VotingService.OnChange -= OnUpdate;
        }

        void OnUpdate()
        {
            _descriptionFields = VotingService.VotingServiceData.WorkItem?.DescriptionFields?
                .ConvertAll(field => new MarkupDescriptionField(field))
                .Where(field => !string.IsNullOrWhiteSpace(field.Value));
            InvokeAsync(StateHasChanged);
        }
    }
}