using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library
{
    public static class Constants
    {
        public const string HUB_COMMAND_ADD_VOTES = nameof(VotingService.AddStorySizeVotes);
        public const string HUB_COMMAND_CLEAR_VOTES = nameof(VotingService.ClearStorySizeVotes);
        public const string HUB_COMMAND_REVEAL_VOTES = nameof(VotingService.RevealVotes);
        public const string HUB_COMMAND_UPDATE_WORK_ITEM = nameof(VotingService.UpdateWorkItem);
        public const string HUB_COMMAND_NEW_CONNECTION = "NewConnection";
        public const string HUB_COMMAND_CANCEL_TIMER = nameof(VotingService.CancelTimer);
        public const string HUB_COMMAND_TIME_REMAINING = nameof(VotingService.TimeRemaining);

        public const int MAX_SIGNALR_SIZE = 30000;
    }
}