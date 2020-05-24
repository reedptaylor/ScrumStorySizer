using System;
using System.Linq;
using Microsoft.AspNetCore.Components;
using PokerCardsShared.Enums;
using PokerCardsShared.Models;
using PokerCardsShared.Services;

namespace PokerCardsShared.Pages
{
    public partial class User : IDisposable
    {
        [Inject] IVotingService PokerVote { get; set; }

        private void AddVote(StorySize vote)
        {
            PokerVote.AddStorySizeVotes(new SizeVote() { Size = vote, User = Username });
            ChangeVote = false;
            IsDisabled = true;
        }

        protected bool IsDisabled { get; set; } = false;

        private string Username { get; set; }

        private bool CanVote
        {
            get
            {
                return !(PokerVote.StorySizeVotes.Any(item => item.User == Username) || string.IsNullOrWhiteSpace(PokerVote.StoryName));
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
