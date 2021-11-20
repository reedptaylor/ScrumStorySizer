using System.Text;
using System.Text.Json.Serialization;

namespace ScrumStorySizer.Library.Models
{
    public class DevOpsCredential : IScrumMasterSettings
    {
        public string AccessToken { get; set; }

        public string Organization { get; set; }

        public string Project { get; set; }

        public bool IsEnabled { get; set; }

        public bool ShowDescription { get; set; }

        public List<string> TagsToAdd { get; set; } = new();

        public List<string> TagsToRemove { get; set; } = new();

        public string NewState { get; set; }

        public string Password { set { AccessToken = value; } } // Legacy access token field

        [JsonIgnore]
        public string BasicAuth => Convert.ToBase64String(Encoding.UTF8.GetBytes($":{AccessToken}")); // Do not serialize but make it available for code

        [JsonIgnore]
        public string SerialTagsToAdd
        {
            get =>  string.Join(";", TagsToAdd ?? new());
            set
            {
                TagsToAdd = value?.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new();
            }
        }

        [JsonIgnore]
        public string SerialTagsToRemove
        {
            get =>  string.Join(";", TagsToRemove ?? new());
            set
            {
                TagsToRemove = value?.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList() ?? new();
            }
        }
    }
}

