namespace ScrumStorySizer.Library.Components
{
    public partial class MessagePopUp // Modal for displaying messages
    {
        private string message = string.Empty;
        private bool showing = false;
        private bool isMarkUp = false; // Allow for raw HTML to be displayed

        public void ShowMessage(string msg, bool isMarkUp = false)
        {
            message = msg;
            showing = true;
            this.isMarkUp = isMarkUp;
            StateHasChanged();
        }
    }
}
