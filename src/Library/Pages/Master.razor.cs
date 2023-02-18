using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Components;
using ScrumStorySizer.Library.Enums;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class Master : IDisposable // Scrum Master Page
    {
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }
        [CascadingParameter(Name = "_spinner")] public Spinner _spinner { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected IVotingService VotingService { get; set; }

        private DevOpsCredential DevOpsCredential { get; set; } = new();
        private TeamMemberSettings TeamMemberSettings { get; set; } = new();
        private DevOpsStory _devOpsStoryRef;

        protected bool ShowResultsDisabled => VotingService.VotingServiceData.StorySizeVotes.Count == 0 || VotingService.VotingServiceData.ShowVotes;

        protected bool TimerActive { get; set; }

        protected void StartTimer(int seconds)
        {
            TimerActive = true;
            _ = Task.Run(async () => // Run timer in background thread
            {
                while (seconds > 0 && TimerActive)
                {
                    VotingService.TimeRemaining(seconds);
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
            VotingService.CancelTimer();
            TimerActive = false;
        }

        protected void RevealVotes()
        {
            VotingService.RevealVotes();
            TimerActive = false;
        }

        protected void ClearVotes()
        {
            VotingService.ClearStorySizeVotes();
        }

        protected void UpdateWorkItem(WorkItem workItem)
        {
            VotingService.UpdateWorkItem(workItem);
            TimerActive = false;
        }

        protected void NewStory()
        {
            ClearVotes();
            UpdateWorkItem(new WorkItem());
            _devOpsStoryRef?.NewStory(); // Clear fields in DevOps component (if active)
        }

        private async Task SaveVote(StorySize size)
        {
            using var httpClient = new HttpClient();
            IWorkItemClient workItemClient = new DevOpsClient(httpClient, NavigationManager, DevOpsCredential);
            try
            {
                _spinner.Set(true);
                await workItemClient.SizeWorkItem(VotingService.VotingServiceData.WorkItem?.Id ?? "0", (int)size);
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
                _messagePopUp.ShowMessage("Unable to update work item.");
            }
        }

        protected override void OnInitialized()
        {
            VotingService.OnChange += OnUpdate;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                DevOpsCredential = await Helper.GetScrumMasterSettings<DevOpsCredential>(JSRuntime);
                TeamMemberSettings = await Helper.GetTeamMemberSettings(JSRuntime);
                await InvokeAsync(StateHasChanged);
            }
        }

        public void Dispose()
        {
            VotingService.OnChange -= OnUpdate;
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
