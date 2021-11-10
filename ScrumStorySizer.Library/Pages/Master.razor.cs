using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Pages
{
    public partial class Master : IDisposable
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected IVotingService PokerVote { get; set; }

        private DevOpsCredential DevOpsCredential { get; set; } = new();

        private bool ShowResultsDisabled
        {
            get
            {
                return PokerVote.StorySizeVotes.Count == 0 || PokerVote.ShowVotes;
            }
        }

        private string StoryName { get; set; }

        private bool TimerState { get; set; }

        private async Task<string> GetStoryName(string id)
        {
            using HttpClient httpClient = new();
            DevOpsClient devOpsClient = new(httpClient, NavigationManager, DevOpsCredential);
            WorkItem workItem = await devOpsClient.GetWorkItem(id);
            return workItem.Title;
        }

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

        protected async override Task OnInitializedAsync()
        {
            PokerVote.OnChange += OnUpdate;
            string scrumMasterSettings = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "devops-auth");
            if (!string.IsNullOrWhiteSpace(scrumMasterSettings))
            {
                try
                {
                    scrumMasterSettings = Encoding.UTF8.GetString(Convert.FromBase64String(scrumMasterSettings));
                    DevOpsCredential = JsonSerializer.Deserialize<DevOpsCredential>(scrumMasterSettings) ?? new();
                }
                catch { }
            }
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
