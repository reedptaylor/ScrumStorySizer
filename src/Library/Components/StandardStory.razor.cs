using System;
using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StandardStory : IDisposable // Component for entering a Work Item name instead of using DevOps integration
    {
        [Inject] protected IVotingService VotingService { get; set; }

        private string _storyName;
        private string _serverStoryName;
        private bool _titleFocus;

        private void UpdateStoryName(ChangeEventArgs e)
        {
            VotingService.UpdateWorkItem(new WorkItem() { Title = e.Value?.ToString() ?? string.Empty }); // Set only the title
        }

        private void ReconcileStoryName() // Sync story names between Scrum Masters if multiple Scrum Masters hav the page open (edge case)
        {
            _serverStoryName = VotingService.VotingServiceData.WorkItem?.Title ?? string.Empty;
            if (!_titleFocus)
                _storyName = _serverStoryName;
            InvokeAsync(StateHasChanged);
        }

        protected override void OnInitialized()
        {
            VotingService.OnChange += ReconcileStoryName;
            _storyName = VotingService.VotingServiceData.WorkItem?.Title ?? string.Empty;
            _serverStoryName = VotingService.VotingServiceData.WorkItem?.Title ?? string.Empty;
        }

        public void Dispose()
        {
            VotingService.OnChange -= ReconcileStoryName;
        }
    }
}
