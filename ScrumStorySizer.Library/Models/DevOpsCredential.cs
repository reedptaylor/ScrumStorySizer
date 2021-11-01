using System;
using System.Text;
using System.Text.Json.Serialization;

namespace ScrumStorySizer.Library.Models
{
    public class DevOpsCredential
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Organization { get; set; }

        public string Project { get; set; }

        [JsonIgnore]
        public string BasicAuth => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
    }
}

