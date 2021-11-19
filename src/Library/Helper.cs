using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using System.Text;
using System.Text.Json;

namespace ScrumStorySizer.Library
{
    public static class Helper // Static class for helper methods
    {
        public static async Task<T> GetScrumMasterSettings<T>(IJSRuntime jsRuntime) where T : IScrumMasterSettings, new()
        {
            T settings = new();
            string scrumMasterSettings = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "devops-auth");

            if (!string.IsNullOrWhiteSpace(scrumMasterSettings))
            {
                try
                {
                    scrumMasterSettings = Encoding.UTF8.GetString(Convert.FromBase64String(scrumMasterSettings));
                    settings = JsonSerializer.Deserialize<T>(scrumMasterSettings) ?? new();
                }
                catch { }
            }

            return settings;
        }

        public static async Task<TeamMemberSettings> GetTeamMemberSettings(IJSRuntime jsRuntime)
        {
            TeamMemberSettings settings = new();
            string teamMemberSettings = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "team-member-settings");

            if (!string.IsNullOrWhiteSpace(teamMemberSettings))
            {
                try
                {
                    teamMemberSettings = Encoding.UTF8.GetString(Convert.FromBase64String(teamMemberSettings));
                    settings = JsonSerializer.Deserialize<TeamMemberSettings>(teamMemberSettings) ?? new();
                }
                catch { }
            }

            return settings;
        }
    }
}