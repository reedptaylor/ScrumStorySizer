using System;
using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StandardStory : IDisposable
    {
        [Inject] protected IVotingService PokerVote { get; set; }

        private string _storyName;
        private string _serverStoryName;
        private bool _titleFocus;

        private void UpdateStoryName(ChangeEventArgs e)
        {
            PokerVote.UpdateWorkItem(new WorkItem() { Title = e.Value?.ToString() ?? string.Empty });
        }

        private void ReconcileStoryName()
        {
            _serverStoryName = PokerVote.WorkItem?.Title ?? string.Empty;
            if (!_titleFocus)
                _storyName = _serverStoryName;
            InvokeAsync(StateHasChanged);
        }

        protected override void OnInitialized()
        {
            PokerVote.OnChange += ReconcileStoryName;
            _storyName = PokerVote.WorkItem?.Title ?? string.Empty;
            _serverStoryName = PokerVote.WorkItem?.Title ?? string.Empty;
        }

        public void Dispose()
        {
            PokerVote.OnChange -= ReconcileStoryName;
        }
    }
}
