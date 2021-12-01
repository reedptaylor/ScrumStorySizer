using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StoryDetails : IDisposable
    {
        [Inject] IVotingService PokerVote { get; set; }

        private MarkupString _description = new();
        private MarkupString _acceptanceCriteria = new();

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
            _description = PokerVote.WorkItem?.Description.SanitizeHTML().ConvertToMarkupString() ?? new();
            _acceptanceCriteria = PokerVote.WorkItem?.AcceptanceCriteria.SanitizeHTML().ConvertToMarkupString() ?? new();
            InvokeAsync(StateHasChanged);
        }
    }
}