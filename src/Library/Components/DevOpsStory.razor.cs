using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using ScrumStorySizer.Library.Utilities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Components
{
    public partial class DevOpsStory // Component for entering a DevOps Work Item ID instead of Name
    {
        [CascadingParameter(Name = "_messagePopUp")] public MessagePopUp _messagePopUp { get; set; }
        [CascadingParameter(Name = "_spinner")] public Spinner _spinner { get; set; }

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
                _spinner.Set(true);
                PokerVote.UpdateWorkItem(await workItemClient.GetWorkItem(_workItemId)); // Send value to all connected clients
                PokerVote.ClearStorySizeVotes();
                _spinner.Set(false);
            }
            catch (UnauthorizedAccessException)
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Unable to authenticate your DevOps credentials. Please verify them in settings.");
            }
            catch
            {
                _spinner.Set(false);
                _messagePopUp.ShowMessage("Unable to get work item with that ID.");
            }
        }

        private async Task IdKeyDown(KeyboardEventArgs e) // Allow look up on pressing enter
        {
            if (e.Key == "Enter")
            {
                await JSRuntime.BlurUtil(_workItemIdInput);
            }
        }

        public void NewStory() // Used by parent component to clear out text in this component (if active)
        {
            _workItemId = string.Empty;
        }
    }
}
