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
        [Inject] IVotingService PokerVote { get; set; }

        private void AddVote(StorySize vote)
        {
            PokerVote.AddStorySizeVotes(new SizeVote() { Size = vote, User = Username });
            ChangeVote = false;
            NameDisabled = true; // Don't allow the user to change their name after voting
            jumboclass = "jumbo-shrink"; // Shrink jumbo after voting
        }

        private TeamMemberSettings TeamMemberSettings { get; set; } = new();

        private string jumboclass = "";

        protected bool NameDisabled { get; set; } = false;

        private string Username { get; set; }

        private bool VoteDisabled => string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(PokerVote.WorkItem?.Title);

        private bool ChangeVote { get; set; } = false;

        void OnUpdate()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        protected async override Task OnInitializedAsync()
        {
            PokerVote.OnChange += OnUpdate;
            TeamMemberSettings = await Helper.GetTeamMemberSettings(JSRuntime);
            Username = TeamMemberSettings.DefaultDisplayName;
        }

        public void Dispose()
        {
            PokerVote.OnChange -= OnUpdate;
        }
    }
}
