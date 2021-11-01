using System;
using System.Net.Http;
using System.Threading.Tasks;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Services
{
    public interface IWorkItemClient
    {
        public Task<WorkItem> GetWorkItem(string auth, string id);

        public Task SizeWorkItem(string auth, string id, int size);
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

