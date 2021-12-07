using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class About : IDisposable
    {
        [Inject] protected IVotingService VotingService { get; set; }

        private void InvokeStateHasChanged() => InvokeAsync(StateHasChanged);

        override protected void OnInitialized()
        {
            VotingService.OnChange += InvokeStateHasChanged;
        }

        public void Dispose()
        {
            VotingService.OnChange -= InvokeStateHasChanged;
        }
    }
}