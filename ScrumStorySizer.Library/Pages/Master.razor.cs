using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
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

        private bool TimerState { get; set; }

        private async Task StartTimer(int seconds)
        {
            TimerState = true;
            while (seconds > 0 && TimerState)
            {
                PokerVote.TimeRemaining(seconds);
                seconds--;
                await Task.Delay(1000);
            }
            if (TimerState)
            {
                PokerVote.RevealVotes();
                TimerState = false;
            }
        }

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
