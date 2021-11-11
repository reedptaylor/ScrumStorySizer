using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Components
{
    public class StoryBase : ComponentBase, IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] protected IVotingService PokerVote { get; set; }

        protected bool ShowResultsDisabled
        {
            get
            {
                return PokerVote.StorySizeVotes.Count == 0 || PokerVote.ShowVotes;
            }
        }

        protected bool TimerActive { get; set; }

        protected void StartTimer(int seconds)
        {
            TimerActive = true;
            _ = Task.Run(async () =>
            {
                while (seconds > 0 && TimerActive)
                {
                    PokerVote.TimeRemaining(seconds);
                    seconds--;
                    await Task.Delay(1000);
                }
                if (TimerActive)
                {
                    RevealVotes();
                }
            });
        }

        public void CancelTimer()
        {
            PokerVote.CancelTimer();
            TimerActive = false;
        }

        protected void RevealVotes()
        {
            PokerVote.RevealVotes();
            TimerActive = false;
        }

        protected void ClearVotes()
        {
            PokerVote.ClearStorySizeVotes();
        }

        protected void UpdateWorkItem(WorkItem workItem)
        {
            PokerVote.UpdateWorkItem(workItem);
            TimerActive = false;
        }

        protected void NewStory()
        {
            ClearVotes();
            UpdateWorkItem(new WorkItem());
        }

        protected void OnUpdate()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        protected override void OnInitialized()
        {
            PokerVote.OnChange += OnUpdate;
        }

        public virtual void Dispose()
        {
            PokerVote.OnChange -= OnUpdate;
        }
    }
}
