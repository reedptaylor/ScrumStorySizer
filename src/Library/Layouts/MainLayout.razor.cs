using Microsoft.AspNetCore.Components;

namespace ScrumStorySizer.Library.Layouts
{
    public partial class MainLayout
    {
        [Inject] protected NavigationManager NavigationManager { get; set; }

        protected string CurrentPage => NavigationManager?.ToBaseRelativePath(NavigationManager.Uri)?.Split('/')?.FirstOrDefault() ?? string.Empty;
    }
}