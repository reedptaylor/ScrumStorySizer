using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System.Text.Json;

namespace ScrumStorySizer.Library.Pages
{
    public partial class Settings // Settings Page
    {
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }
        [CascadingParameter(Name = "_spinner")] public Spinner _spinner { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        public DevOpsCredential DevOpsCredential { get; set; } = new();
        public TeamMemberSettings TeamMemberSettings { get; set; } = new();

        private bool showCredential = false;

        private readonly string patHelpText = "Personal Access Token (PAT) requires Work Items: read and write permission.";
        private readonly string tagsToAddHelpText = "Semicolon delimited list of tags to add to the work item after it is sized. Leave blank for no change.";
        private readonly string tagsToRemoveHelpText = "Semicolon delimited list of tags to remove from the work item after it is sized. Leave blank for no change.";
        private readonly string stateHelpText = "State to change work item to after it is sized. Leave blank for no change.";

        private async Task SaveCredential() // Save to localStorage
        {
            string auth = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(DevOpsCredential));
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "devops-auth", auth);
        }

        private async Task SetEnabled(bool value) // Test credential if changing to enabled otherwise just save
        {
            DevOpsCredential.IsEnabled = value;
            if (!DevOpsCredential.IsEnabled)
            {
                await SaveCredential();
            }
            else if (!string.IsNullOrWhiteSpace(DevOpsCredential.AccessToken) && !string.IsNullOrWhiteSpace(DevOpsCredential.Organization)
                && !string.IsNullOrWhiteSpace(DevOpsCredential.Project))
            {
                await SubmitCredential();
            }
        }

        private async Task SubmitCredential() // Test credential with DevOps
        {
            using var httpClient = new HttpClient();
            IWorkItemClient workItemClient = new DevOpsClient(httpClient, NavigationManager, DevOpsCredential);
            try
            {
                _spinner.Set(true);
                IEnumerable<string> tags = (DevOpsCredential.TagsToAdd ?? new()).Concat(DevOpsCredential.TagsToRemove ?? new());
                await workItemClient.TestAuthentication(tags, DevOpsCredential.NewState);
                await SaveCredential();
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Credentials are saved.");
            }
            catch (UnauthorizedAccessException)
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Invalid credentials provided.");
            }
            catch (WorkItemClientException ex)
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage(ex.Message);
            }
            catch (Exception ex)
            {
                _spinner.Set(false);
                Console.WriteLine("Error: " + ex.InnerException?.Message ?? ex.Message);
                _messagePopUp.ShowMessage("Unable to save credentials.");
            }
        }

        private async Task SaveTeamMemberSettings() // Save to localStorage
        {
            try
            {
                string json = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(TeamMemberSettings));
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "team-member-settings", json);
                _messagePopUp.ShowMessage("Settings are saved.");
            }
            catch
            {
                _messagePopUp.ShowMessage("Settings are invalid.");
            }
        }

        private void ShowPatHelp() // Open popup with help text
        {
            _messagePopUp.ShowMessage(patHelpText + " <a href=\"https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate\" target=\"_blank\">Click here for help.</a>", true);
        }

        protected async override Task OnInitializedAsync()
        {
            // Load settings from localStorage
            DevOpsCredential = await Helper.GetScrumMasterSettings<DevOpsCredential>(JSRuntime);
            TeamMemberSettings = await Helper.GetTeamMemberSettings(JSRuntime);
        }
    }
}
