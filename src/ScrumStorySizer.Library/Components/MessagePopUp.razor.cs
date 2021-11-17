namespace ScrumStorySizer.Library.Components
{
    public partial class MessagePopUp
    {
        private string message = string.Empty;
        private bool showing = false;
        private bool isMarkUp = false;

        public void ShowMessage(string msg, bool isMarkUp = false)
        {
            message = msg;
            showing = true;
            this.isMarkUp = isMarkUp;
            StateHasChanged();
        }
    }
}
