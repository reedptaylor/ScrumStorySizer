using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public class ClientVotingService : IVotingService // State and connection manager for clients
    {
        public HubConnection HubConnection { get; private set; }

        public ClientVotingService(string hubUrl)
        {
            // Declare SignalR Hub Connection and set up event handlers
            HubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            HubConnection.On<SizeVote>(Constants.HUB_COMMAND_ADD_VOTES, (vote) =>
            {
                VotingServiceData.StorySizeVotes.RemoveAll(item => item.User == vote.User);
                VotingServiceData.StorySizeVotes.Add(vote);
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_CLEAR_VOTES, () =>
            {
                VotingServiceData.StorySizeVotes.Clear();
                VotingServiceData.ShowVotes = false;
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_REVEAL_VOTES, () =>
            {
                VotingServiceData.ShowVotes = true;
                VotingServiceData.TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<WorkItem>(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, (workItem) =>
            {
                VotingServiceData.WorkItem = workItem;
                VotingServiceData.TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<VotingServiceData>(Constants.HUB_COMMAND_NEW_CONNECTION, (votingServiceData) =>
            {
                VotingServiceData = votingServiceData;
                NotifyDataChanged();
            });

            HubConnection.On(Constants.HUB_COMMAND_CANCEL_TIMER, () =>
            {
                VotingServiceData.TimeLeft = 0;
                NotifyDataChanged();
            });

            HubConnection.On<int>(Constants.HUB_UPDATE_TIME_REMAINING, (seconds) =>
            {
                VotingServiceData.TimeLeft = seconds;
                NotifyDataChanged();
            });

            HubConnection.On<int>(Constants.HUB_UPDATE_CONNECTED_CLIENTS, (count) =>
            {
                VotingServiceData.ConnectedClients = count;
                NotifyDataChanged();
            });
        }

        // In Memory State Data
        public VotingServiceData VotingServiceData { get; set; } = new();

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
            workItem.TruncateObject(); // Truncate the object if it will be to big to send over SignalR
            HubConnection.SendAsync(Constants.HUB_COMMAND_UPDATE_WORK_ITEM, workItem);
        }

        public void CancelTimer()
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_CANCEL_TIMER);
        }

        public void StartTimer()
        {
            HubConnection.SendAsync(Constants.HUB_COMMAND_START_TIMER);
        }

        private void NotifyDataChanged()
        {
            OnChange?.Invoke();
        }
    }
}
