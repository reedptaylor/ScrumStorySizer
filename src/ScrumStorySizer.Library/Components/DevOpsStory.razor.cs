using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Components
{
    public partial class DevOpsStory
    {
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }

        [Parameter] public DevOpsCredential DevOpsCredential { get; set; }
        
        [Inject] protected IVotingService PokerVote { get; set; }
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        private string _workItemId;
        protected ElementReference _workItemIdInput;

        private async Task GetWorkItem()
        {
            if (string.IsNullOrWhiteSpace(_workItemId)) return;
            _workItemId = _workItemId.Trim();

            using var httpClient = new HttpClient();
            IWorkItemClient workItemClient = new DevOpsClient(httpClient, NavigationManager, DevOpsCredential);
            try
            {
                PokerVote.UpdateWorkItem(await workItemClient.GetWorkItem(_workItemId));
            }
            catch (UnauthorizedAccessException)
            {
                _messagePopUp.ShowMessage("Unable to authenticate your DevOps credentials. Please verify them in settings.");
            }
            catch
            {
                _messagePopUp.ShowMessage("Unable to get work item with that ID.");
            }
        }

        private async Task IdKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await JSRuntime.InvokeVoidAsync("utils.blur", _workItemIdInput);
            }
        }

        public void NewStory()
        {
            _workItemId = string.Empty;
        }
    }
}
