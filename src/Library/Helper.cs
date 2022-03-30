using Ganss.XSS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using System.Text;
using System.Text.Json;

namespace ScrumStorySizer.Library
{
    public static class Helper // Static class for helper methods
    {
        public static async Task<(DevOpsCredential credential, bool IsEnabled)> GetCurrentScrumMasterSettings(this IJSRuntime jsRuntime)
        {
            DevOpsCredentialList settingsList = await jsRuntime.GetScrumMasterSettings();
            DevOpsCredential settings = settingsList.Credentials.FirstOrDefault(c => c.IsSelected) ?? (settingsList.Credentials.FirstOrDefault() ?? new DevOpsCredential());
            return (settings, settingsList.IsEnabled);
        }

        public static async Task<DevOpsCredentialList> GetScrumMasterSettings(this IJSRuntime jsRuntime)
        {
            DevOpsCredentialList settingsList = new();
            string scrumMasterSettings = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "scrum-master-settings");

            if (!string.IsNullOrWhiteSpace(scrumMasterSettings))
            {
                try
                {
                    scrumMasterSettings = Encoding.UTF8.GetString(Convert.FromBase64String(scrumMasterSettings));
                    settingsList = JsonSerializer.Deserialize<DevOpsCredentialList>(scrumMasterSettings) ?? new();
                }
                catch { }
            }

            return settingsList;
        }

        public static async Task<TeamMemberSettings> GetTeamMemberSettings(this IJSRuntime jsRuntime)
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

        public static MarkupString ConvertToMarkupString(this string html) => new MarkupString(html);

        public static string SanitizeHTML(this string html, bool allowCss = false) // Default allowCss to false to help display better in app and trim down on total size
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            HtmlSanitizer sanitizer = new(allowedCssProperties: allowCss ? null : new List<string>()); // Use GANSS library to sanitize HTML to prevent XSS attacks
            return sanitizer.Sanitize(html);
        }

        public static string LimitByteLength(this string message, ref int remainingByteSize) // Helper method to get a string with a maximum byte length
        {
            if (string.IsNullOrWhiteSpace(message) || remainingByteSize <= 0)
                return string.Empty;

            int messageByteSize = Encoding.UTF8.GetByteCount(message);

            if (messageByteSize > remainingByteSize) // Message size too large so we need to trim it
            {
                Encoder encoder = Encoding.UTF8.GetEncoder();
                byte[] buffer = new byte[remainingByteSize];
                encoder.Convert(message.AsSpan(), buffer.AsSpan(), false, out _, out messageByteSize, out _);
                message = Encoding.UTF8.GetString(buffer, 0, messageByteSize);
            }

            remainingByteSize -= messageByteSize; // Update remaining byte size
            return message;
        }

        public static string ToRelativeTimeStamp(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1)
                return $"Less than a minute";

            if (timeSpan.TotalHours < 1)
                return $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")}";

            if (timeSpan.TotalDays < 1)
                return $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}";

            if (timeSpan.TotalDays < 7)
                return $"{timeSpan.Days} day{(timeSpan.Days == 1 ? string.Empty : "s")}";

            if (timeSpan.TotalDays < 365)
                return $"{timeSpan.Days / 7} week{((timeSpan.Days / 7) == 1 ? string.Empty : "s")}";

            return $"{timeSpan.Days / 365} year{((timeSpan.Days / 365) == 1 ? string.Empty : "s")}";
        }
    }
}