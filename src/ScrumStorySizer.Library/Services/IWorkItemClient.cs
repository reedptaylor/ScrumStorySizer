using System;
using System.Net.Http;
using System.Threading.Tasks;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IWorkItemClient
    {
        Task TestAuthentication();

        Task<WorkItem> GetWorkItem(string id);

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

