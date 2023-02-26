using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class About : IDisposable
    {
        [Inject] protected IVotingService VotingService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] PersistentComponentState ApplicationState { get; set; }

        private void InvokeStateHasChanged() => InvokeAsync(StateHasChanged);
        private MarkupString LatestVersion { get; set; }
        private string Uptime { get; set; } = "Loading...";
        private PersistingComponentStateSubscription persistingSubscription;

        private Task PersistData()
        {
            ApplicationState.PersistAsJson($"{nameof(About)}_{nameof(Uptime)}", Uptime);
            ApplicationState.PersistAsJson($"{nameof(About)}_{nameof(LatestVersion)}", LatestVersion.Value);

            return Task.CompletedTask;
        }

        override protected async Task OnInitializedAsync()
        {
            VotingService.OnChange += InvokeStateHasChanged;
            persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

            if (ApplicationState.TryTakeFromJson<string>($"{nameof(About)}_{nameof(Uptime)}", out string uptime)
                && ApplicationState.TryTakeFromJson<string>($"{nameof(About)}_{nameof(LatestVersion)}", out string latestVersion))
            {
                Uptime = uptime;
                LatestVersion = latestVersion.ConvertToMarkupString();
            }
            else
            {
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(NavigationManager.BaseUri);

                Task<string> uptimeTask = httpClient.GetStringAsync("/uptime");
                Task<string> releaseNotesTask = httpClient.GetStringAsync("/release-notes?format=simple&version=latest");

                Uptime = await uptimeTask;
                LatestVersion = Helper.ConvertToMarkupString(Helper.SanitizeHTML(await releaseNotesTask));
            }
        }

        public void Dispose()
        {
            VotingService.OnChange -= InvokeStateHasChanged;
        }
    }
}