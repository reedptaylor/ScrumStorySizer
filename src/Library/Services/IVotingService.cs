﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IVotingService
    {
        HubConnection HubConnection { get; }
        VotingServiceData VotingServiceData { get; set; }
        event Action OnChange;
        void AddStorySizeVotes(SizeVote vote);
        void ClearStorySizeVotes();
        void UpdateWorkItem(WorkItem workItem);
        void RevealVotes();
        void StartTimer();
        void CancelTimer();
    }
}
