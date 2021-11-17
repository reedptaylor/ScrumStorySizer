using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Components;
using ScrumStorySizer.Library.Enums;
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
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }
        [CascadingParameter(Name = "_spinner")] public Spinner _spinner { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected IVotingService PokerVote { get; set; }

        private DevOpsCredential DevOpsCredential { get; set; }
        private DevOpsStory _devOpsStoryRef;

        protected bool ShowResultsDisabled => PokerVote.StorySizeVotes.Count == 0 || PokerVote.ShowVotes;

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
            _devOpsStoryRef?.NewStory();
        }

        private async Task SaveVote(StorySize size)
        {
            using var httpClient = new HttpClient();
            IWorkItemClient workItemClient = new DevOpsClient(httpClient, NavigationManager, DevOpsCredential);
            try
            {
                _spinner.Set(true);
                await workItemClient.SizeWorkItem(PokerVote.WorkItem?.Id, (int)size);
                NewStory();
                _spinner.Set(false);
            }
            catch (UnauthorizedAccessException)
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Unable to authenticate your DevOps credentials. Please verify them in settings.");
            }
            catch
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Unable to update work item with that ID.");
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
                    DevOpsCredential = JsonSerializer.Deserialize<DevOpsCredential>(scrumMasterSettings);
                    OnUpdate();
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
