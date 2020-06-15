using System;
using Microsoft.AspNetCore.Components;
using PokerCardsShared.Services;

namespace PokerCardsShared.Pages
{
    public partial class Master : IDisposable
    {

        [Inject] IVotingService PokerVote { get; set; }

        private bool ShowResultsDisabled
        {
            get
            {
                return PokerVote.StorySizeVotes.Count == 0 || PokerVote.ShowVotes;
            }
        }

        private string StoryName { get; set; }

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
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
    }
}
