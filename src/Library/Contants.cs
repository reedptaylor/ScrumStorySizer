using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library
{
    public static class Constants
    {
        public const string HUB_COMMAND_ADD_VOTES = nameof(IVotingService.AddStorySizeVotes);
        public const string HUB_COMMAND_CLEAR_VOTES = nameof(IVotingService.ClearStorySizeVotes);
        public const string HUB_COMMAND_REVEAL_VOTES = nameof(IVotingService.RevealVotes);
        public const string HUB_COMMAND_UPDATE_WORK_ITEM = nameof(IVotingService.UpdateWorkItem);
        public const string HUB_COMMAND_NEW_CONNECTION = "NewConnection";
        public const string HUB_COMMAND_CANCEL_TIMER = nameof(IVotingService.CancelTimer);
        public const string HUB_COMMAND_TIME_REMAINING = nameof(IVotingService.TimeRemaining);
        public const string HUB_UPDATE_CONNECTED_CLIENTS = "UpdateConnectedClients";

        public const int MAX_SIGNALR_SIZE = 254000; // Max size for a SignalR message is 256KB (manually set) so we have a 2KB buffer for the JSON payload

        public const string DEFAULT_PBI_NAME = "Product Backlog Item";
    }
}