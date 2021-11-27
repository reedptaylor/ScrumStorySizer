using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Components
{
    public partial class StoryDetails : IDisposable
    {
        [Inject] IVotingService PokerVote { get; set; }

        protected override void OnInitialized()
        {
            PokerVote.OnChange += OnUpdate;
        }

        public void Dispose()
        {
            PokerVote.OnChange -= OnUpdate;
        }

        void OnUpdate()
        {
            InvokeAsync(StateHasChanged);
        }
    }
}