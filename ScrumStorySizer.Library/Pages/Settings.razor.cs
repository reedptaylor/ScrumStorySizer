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

        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        public DevOpsCredential DevOpsCredential { get; set; } = new DevOpsCredential();

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
                await workItemClient.TestAuthentication();
                await SaveCredential();
                _messagePopUp.ShowMessage("Credentials are saved.");
            }
            catch
            {
                _messagePopUp.ShowMessage("Credentials are invalid.");
            }
        }

        protected async override Task OnInitializedAsync()
        {
            string auth = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "devops-auth");
            if (!string.IsNullOrWhiteSpace(auth))
            {
                try
                {
                    auth = Encoding.UTF8.GetString(Convert.FromBase64String(auth));
                    DevOpsCredential = JsonSerializer.Deserialize<DevOpsCredential>(auth) ?? new();
                }
                catch { }
            }
        }
    }
}
