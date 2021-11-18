using System;
using System.Collections.Generic;
using ScrumStorySizer.Library.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Services
{
    public class VotingService : IVotingService // State and connection manager for clients
    {
        public HubConnection HubConnection { get; private set; }

        public VotingService(NavigationManager navigationManager)
        {
            // Declare SignalR Hub Connection and set up event handlers
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/voteHub"))
                .WithAutomaticReconnect()
                .Build();

            HubConnection.On<SizeVote>(Constants.HUB_COMMAND_ADD_VOTES, (vote) =>
            {
                StorySizeVotes.RemoveAll(item => item.User == vote.User);
                StorySizeVotes.Add(vote);
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_CLEAR_VOTES, () =>
            {
                StorySizeVotes.Clear();
                ShowVotes = false;
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_REVEAL_VOTES, () =>
            {
                ShowVotes = true;
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<WorkItem>(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, (workItem) =>
            {
                WorkItem = workItem;
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<WorkItem, List<SizeVote>, bool>(Constants.HUB_COMMAND_NEW_CONNECTION, (workItem, storySizeVotes, showVotes) =>
            {
                WorkItem = workItem;
                StorySizeVotes = storySizeVotes;
                ShowVotes = showVotes;
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_CANCEL_TIMER, () =>
            {
                TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<int>(Constants.HUB_COMMAND_TIME_REMAINING, (seconds) =>
            {
                TimeLeft = seconds;
                NotifyDataChanged();
            });
        }

        // In Memory State Data
        public WorkItem WorkItem { get; set; } = new WorkItem();

        public List<SizeVote> StorySizeVotes { get; private set; } = new List<SizeVote>();

        public bool ShowVotes { get; private set; }

        public int TimeLeft { get; set; } = 0;

        public event Action OnChange;

        // Methods to send updates to the server/other clients
        public void AddStorySizeVotes(SizeVote vote)
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_ADD_VOTES, vote);
        }

        public void ClearStorySizeVotes()
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_CLEAR_VOTES);
        }

        public void RevealVotes()
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_REVEAL_VOTES);
        }

        public void UpdateWorkItem(WorkItem workItem)
        {
            if (workItem is null) workItem = new();
            HubConnection.SendAsync(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, workItem);
        }

        public void CancelTimer()
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_CANCEL_TIMER);
        }

        public void TimeRemaining(int seconds)
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_TIME_REMAINING, seconds);
        }

        private void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
