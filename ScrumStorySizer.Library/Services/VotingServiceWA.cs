using System;
using System.Collections.Generic;
using ScrumStorySizer.Library.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Services
{
    public class VotingServiceWA : IVotingService
    {
        public HubConnection HubConnection { get; private set; }

        public VotingServiceWA(NavigationManager navigationManager)
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/voteHub"))
                .Build();

            HubConnection.On<SizeVote>("ReceiveAddStorySizeVotes", (vote) =>
            {
                StorySizeVotes.RemoveAll(item => item.User == vote.User);
                StorySizeVotes.Add(vote);
                NotifyDataChanged();
            });

            HubConnection.On("ReceiveClearStorySizeVotes", () =>
            {
                StorySizeVotes.Clear();
                ShowVotes = false;
                NotifyDataChanged();
            });

            HubConnection.On("ReceiveRevealVotes", () =>
            {
                ShowVotes = true;
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<string>("ReceiveUpdateStoryName", (name) =>
            {
                StoryName = name;
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<string, List<SizeVote>, bool>("ReceiveCache", (storyName, storySizeVotes, showVotes) =>
            {
                StoryName = storyName;
                StorySizeVotes = storySizeVotes;
                ShowVotes = showVotes;
                NotifyDataChanged();
            });

            HubConnection.On<int>("TimeRemaining", (seconds) =>
            {
                TimeLeft = seconds;
                NotifyDataChanged();
            });

            HubConnection.On("CancelTimer", () =>
            {
                TimeLeft = 0;
                NotifyDataChanged();
            });
        }

        public string StoryName { get; set; }

        public List<SizeVote> StorySizeVotes { get; private set; } = new List<SizeVote>();

        public bool ShowVotes { get; private set; }

        public int TimeLeft { get; set; } = 0;

        public event Action OnChange;

        public void AddStorySizeVotes(SizeVote vote)
        {
            HubConnection.SendAsync("AddStorySizeVotes", vote);
        }

        public void ClearStorySizeVotes()
        {
            HubConnection.SendAsync("ClearStorySizeVotes");
        }

        public void RevealVotes()
        {
            HubConnection.SendAsync("RevealVotes");
        }

        public void UpdateStoryName(string name)
        {
            HubConnection.SendAsync("UpdateStoryName", name);
        }

        public void TimeRemaining(int seconds)
        {
            HubConnection.SendAsync("TimeRemaining", seconds);
        }

        public void CancelTimer()
        {
            HubConnection.SendAsync("CancelTimer");
        }

        private void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
