using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ScrumStorySizer.Library.Components
{
    public partial class Spinner
    {
        public bool Show { get; private set; }

        public void Set(bool enabled)
        {
            Show = enabled;
            StateHasChanged();
        }
    }
}