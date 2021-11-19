using System;
using System.Text;
using System.Text.Json.Serialization;

namespace ScrumStorySizer.Library.Models
{
    public interface IScrumMasterSettings
    {
        bool IsEnabled { get; set; }

        bool ShowDescription { get; set; }

        List<string> TagsToAdd { get; set; }

        List<string> TagsToRemove { get; set; }

        string NewState { get; set; }

        [JsonIgnore]
        string BasicAuth { get; }
    }
}