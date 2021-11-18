using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Enums;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        protected async override Task OnInitializedAsync() // Load user defaults/settings
        {
            PokerVote.OnChange += OnUpdate;
            string teamMemberSettings = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "team-member-settings");
            if (!string.IsNullOrWhiteSpace(teamMemberSettings))
            {
                try
                {
                    teamMemberSettings = Encoding.UTF8.GetString(Convert.FromBase64String(teamMemberSettings));
                    Username = (JsonSerializer.Deserialize<TeamMemberSettings>(teamMemberSettings) ?? new()).DefaultDisplayName;
                }
                catch { }
            }
        }

        public void Dispose()
        {
            PokerVote.OnChange -= OnUpdate;
        }
    }
}
