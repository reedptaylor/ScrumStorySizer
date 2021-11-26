using Microsoft.AspNetCore.Components;

namespace ScrumStorySizer.Library.Components
{
    public partial class MessagePopUp // Modal for displaying messages
    {
        private MarkupString markupMessage = new();
        private string message = string.Empty;
        private bool showing = false;
        private bool isMarkUp = false; // Allow for raw HTML to be displayed

        public void ShowMessage(string msg, bool isMarkUp = false)
        {
            if (isMarkUp)
                markupMessage = msg.SanitizeHTML().ConvertToMarkupString();
            else
                message = msg;

            showing = true;
            this.isMarkUp = isMarkUp;
            StateHasChanged();
        }
    }
}
