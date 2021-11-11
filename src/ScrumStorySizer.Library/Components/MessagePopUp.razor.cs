namespace ScrumStorySizer.Library.Components
{
    public partial class MessagePopUp
    {
        private string message = string.Empty;
        private bool showing = false;

        public void ShowMessage(string msg)
        {
            message = msg;
            showing = true;
            StateHasChanged();
        }
    }
}
