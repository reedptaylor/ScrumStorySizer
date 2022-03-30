namespace ScrumStorySizer.Library.Models
{
    public class DevOpsCredentialList
    {
        public bool IsEnabled { get; set; }
        public List<DevOpsCredential> Credentials { get; set; } = new();
    }
}

