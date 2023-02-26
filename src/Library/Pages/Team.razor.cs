using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Enums;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class Team : IDisposable // Scrum Team Page
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] IVotingService VotingService { get; set; }
        [Inject] PersistentComponentState ApplicationState { get; set; }

        private void AddVote(StorySize vote)
        {
            VotingService.AddStorySizeVotes(new SizeVote() { Size = vote, User = Username });
            ChangeVote = false;
            NameDisabled = true; // Don't allow the user to change their name after voting
            jumboclass = "jumbo-shrink"; // Shrink jumbo after voting
        }

        private TeamMemberSettings TeamMemberSettings { get; set; } = new();
        private string jumboclass = "";
        private PersistingComponentStateSubscription persistingSubscription;

        protected bool NameDisabled { get; set; } = false;

        private string Username { get; set; }

        private bool VoteDisabled => string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(VotingService.VotingServiceData.WorkItem?.Title);

        private bool ChangeVote { get; set; } = false;

        void OnUpdate() => InvokeAsync(StateHasChanged);

        private Task PersistData()
        {
            ApplicationState.PersistAsJson($"{nameof(Team)}_{nameof(Username)}", Username);

            return Task.CompletedTask;
        }

        protected override void OnInitialized()
        {
            VotingService.OnChange += OnUpdate;

            persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);
            if (ApplicationState.TryTakeFromJson<string>($"{nameof(Team)}_{nameof(Username)}", out string restored))
                Username = restored;
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                TeamMemberSettings = await Helper.GetTeamMemberSettings(JSRuntime);

                if (string.IsNullOrWhiteSpace(Username))
                    Username = TeamMemberSettings.DefaultDisplayName;

                await InvokeAsync(StateHasChanged);
            }
        }

        public void Dispose()
        {
            VotingService.OnChange -= OnUpdate;

            persistingSubscription.Dispose();
        }
    }
}
