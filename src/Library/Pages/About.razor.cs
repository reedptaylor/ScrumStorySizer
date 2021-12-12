using System.Net;
using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class About : IDisposable
    {
        [Inject] protected IVotingService VotingService { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }

        private void InvokeStateHasChanged() => InvokeAsync(StateHasChanged);
        private MarkupString LatestVersion { get; set; }

        override async protected Task OnInitializedAsync()
        {
            VotingService.OnChange += InvokeStateHasChanged;
            LatestVersion = Helper.ConvertToMarkupString(Helper.SanitizeHTML(await HttpClient.GetStringAsync("/release-notes?format=simple&version=latest")));
        }

        public void Dispose()
        {
            VotingService.OnChange -= InvokeStateHasChanged;
        }
    }
}