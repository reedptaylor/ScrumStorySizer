using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IVotingService
    {
        HubConnection HubConnection { get; }
        WorkItem WorkItem { get; set; }
        List<SizeVote> StorySizeVotes { get; }
        bool ShowVotes { get; }
        int ConnectedClients { get; }
        int TimeLeft { get; }
        event Action OnChange;
        void AddStorySizeVotes(SizeVote vote);
        void ClearStorySizeVotes();
        void UpdateWorkItem(WorkItem workItem);
        void RevealVotes();
        void TimeRemaining(int seconds);
        void CancelTimer();
    }
}
