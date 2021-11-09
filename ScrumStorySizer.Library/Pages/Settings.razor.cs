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
        [Inject] protected HttpClient HttpClient { get; set; }

        public DevOpsCredential DevOpsCredential { get; set; } = new DevOpsCredential();

        private async Task SubmitCredential()
        {
            IWorkItemClient workItemClient = new DevOpsClient(HttpClient, DevOpsCredential);
            try
            {
                //todo test azure credential
                await workItemClient.TestAuthentication();
                string auth = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(DevOpsCredential));
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "devops-auth", auth);
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
                    DevOpsCredential = JsonSerializer.Deserialize<DevOpsCredential>(auth);
                }
                catch { }
            }
        }
    }
}
