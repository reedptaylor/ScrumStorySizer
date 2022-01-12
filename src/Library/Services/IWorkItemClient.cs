using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IWorkItemClient
    {
        Task TestAuthentication(IEnumerable<string> tags, string state);

        Task<WorkItem> GetWorkItem(string id, IEnumerable<DescriptionField> extraDescriptionFields = null);

        Task SizeWorkItem(string id, int size);
    }

    public class WorkItemClientException : Exception
    {
        public WorkItemClientException()
        {
        }

        public WorkItemClientException(string message)
            : base(message)
        {
        }

        public WorkItemClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

