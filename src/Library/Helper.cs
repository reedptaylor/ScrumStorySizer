using Ganss.Xss;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Utilities;
using System.Text;
using System.Text.Json;

namespace ScrumStorySizer.Library
{
    public static class Helper // Static class for helper methods
    {
        public static async Task<T> GetScrumMasterSettings<T>(IJSRuntime jsRuntime) where T : IScrumMasterSettings, new()
        {
            T settings = new();

            string scrumMasterSettings="";
            try
            {
                scrumMasterSettings = await jsRuntime.GetItemLocalStorage("devops-auth");
            }
            catch (JSDisconnectedException) { }

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

            string teamMemberSettings = "";
            try
            {
                teamMemberSettings = await jsRuntime.GetItemLocalStorage("team-member-settings");
            }
            catch (JSDisconnectedException) { }

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

        public static string SanitizeHTML(this string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Use GANSS library to sanitize HTML to prevent XSS attacks
            HtmlSanitizer sanitizer = new();
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