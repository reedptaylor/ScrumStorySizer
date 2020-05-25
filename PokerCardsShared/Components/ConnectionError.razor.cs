using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace PokerCardsShared.Components
{
    public partial class ConnectionError
    {
        [Parameter] public HubConnectionState State { get; set; }

        [Inject] NavigationManager NavigationManager { get; set; }

        private string ConnectionState
        {
            get
            {
                return State.ToString();
            }
        }
    }
}
