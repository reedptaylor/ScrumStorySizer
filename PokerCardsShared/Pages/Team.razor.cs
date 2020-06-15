using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using PokerCardsShared.Enums;
using PokerCardsShared.Models;
using PokerCardsShared.Services;

namespace PokerCardsShared.Pages
{
    public partial class Team : IDisposable
    {
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
