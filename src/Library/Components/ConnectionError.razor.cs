using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ScrumStorySizer.Library.Components
{
    public partial class ConnectionError // Modal for displaying bad connection state for SignalR updates
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
