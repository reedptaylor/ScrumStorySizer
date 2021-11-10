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
    public partial class Team : IDisposable
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] IVotingService PokerVote { get; set; }

        private void AddVote(StorySize vote)
        {
            PokerVote.AddStorySizeVotes(new SizeVote() { Size = vote, User = Username });
            ChangeVote = false;
            NameDisabled = true;
            jumboclass = "jumbo-shrink";
        }

        private string jumboclass = "";

        protected bool NameDisabled { get; set; } = false;

        private string Username { get; set; }

        private bool VoteDisabled
        {
            get
            {
                return string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(PokerVote.StoryName);
            }
        }

        private bool ChangeVote { get; set; } = false;

        protected async override Task OnInitializedAsync()
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

        void OnUpdate()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
    }
}
