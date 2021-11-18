using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ScrumStorySizer.Library.Components
{
    public partial class Spinner // Loading spinner for network calls or long running Tasks
    {
        public bool Show { get; private set; }

        public void Set(bool enabled)
        {
            Show = enabled;
            InvokeAsync(StateHasChanged);
        }
    }
}