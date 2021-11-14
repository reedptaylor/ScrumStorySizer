using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Components;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;

namespace ScrumStorySizer.Library.Pages
{
    public partial class Settings
    {
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }
        [CascadingParameter(Name = "_spinner")] public Spinner _spinner { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        public DevOpsCredential DevOpsCredential { get; set; } = new();
        public TeamMemberSettings TeamMemberSettings { get; set; } = new();

        private bool showCredential = false;

        private async Task SaveCredential()
        {
            string auth = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(DevOpsCredential));
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "devops-auth", auth);
        }

        private async Task SetEnabled(bool value)
        {
            DevOpsCredential.IsEnabled = value;
            if (!DevOpsCredential.IsEnabled)
            {
                await SaveCredential();
            }
            else if (!string.IsNullOrWhiteSpace(DevOpsCredential.Username) && !string.IsNullOrWhiteSpace(DevOpsCredential.Password)
                && !string.IsNullOrWhiteSpace(DevOpsCredential.Organization) && !string.IsNullOrWhiteSpace(DevOpsCredential.Project))
            {
                await SubmitCredential();
            }
        }

        private async Task SubmitCredential()
        {
            using var httpClient = new HttpClient();
            IWorkItemClient workItemClient = new DevOpsClient(httpClient, NavigationManager, DevOpsCredential);
            try
            {
                _spinner.Set(true);
                await workItemClient.TestAuthentication();
                await SaveCredential();
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Credentials are saved.");
            }
            catch
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Credentials are invalid.");
            }
        }

        private async Task SaveTeamMemberSettings()
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

        protected async override Task OnInitializedAsync()
        {
            string scrumMasterSettings = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "devops-auth");
            string teamMemberSettings = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "team-member-settings");
            if (!string.IsNullOrWhiteSpace(scrumMasterSettings))
            {
                try
                {
                    scrumMasterSettings = Encoding.UTF8.GetString(Convert.FromBase64String(scrumMasterSettings));
                    DevOpsCredential = JsonSerializer.Deserialize<DevOpsCredential>(scrumMasterSettings) ?? new();
                }
                catch { }
            }
            if (!string.IsNullOrWhiteSpace(teamMemberSettings))
            {
                try
                {
                    teamMemberSettings = Encoding.UTF8.GetString(Convert.FromBase64String(teamMemberSettings));
                    TeamMemberSettings = JsonSerializer.Deserialize<TeamMemberSettings>(teamMemberSettings) ?? new();
                }
                catch { }
            }
        }
    }
}
