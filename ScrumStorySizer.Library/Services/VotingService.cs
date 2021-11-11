using System;
using System.Collections.Generic;
using ScrumStorySizer.Library.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Services
{
    public class VotingService : IVotingService
    {
        public HubConnection HubConnection { get; private set; }

        public VotingService(NavigationManager navigationManager)
        {
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/voteHub"))
                .WithAutomaticReconnect()
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

            HubConnection.On<WorkItem>("ReceiveUpdateWorkItem", (workItem) =>
            {
                WorkItem = workItem;
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<WorkItem, List<SizeVote>, bool>("ReceiveCache", (workItem, storySizeVotes, showVotes) =>
            {
                WorkItem = workItem;
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

        public WorkItem WorkItem { get; set; } = new WorkItem();

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

        public void UpdateWorkItem(WorkItem workItem)
        {
            if (workItem is null) workItem = new();
            HubConnection.SendAsync("UpdateWorkItem", workItem);
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
