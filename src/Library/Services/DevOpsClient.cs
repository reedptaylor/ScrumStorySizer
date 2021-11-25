using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ScrumStorySizer.Library.Services
{
    public class DevOpsClient : IWorkItemClient // Client for getting and setting information in DevOps
    {
        private readonly HttpClient _httpClient;
        private readonly DevOpsCredential _credential;

        private readonly List<string> _tagsToAdd;
        private readonly List<string> _tagsToRemove;
        private readonly string _newState;

        public DevOpsClient(HttpClient httpClient, NavigationManager navigationManager, DevOpsCredential credential)
        {
            _httpClient = httpClient;
            _credential = credential;

            // Set address using Yarp Proxy
            _httpClient.BaseAddress = new Uri($"{navigationManager.BaseUri}devops/{credential.Organization}/{credential.Project}/_apis/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credential.BasicAuth);

            _tagsToAdd = credential.TagsToAdd ?? new();
            _tagsToRemove = credential.TagsToRemove ?? new();
            _newState = credential.NewState;
        }

        public async Task TestAuthentication()
        {
            // Test authentication using get work item with ID = 0. A 403 signifies authentication failure while a 400 signifies success.
            var workItemResponse = await _httpClient.GetAsync($"wit/workitems?ids=0&api-version=6.0");
            if (!workItemResponse.IsSuccessStatusCode && workItemResponse.StatusCode != HttpStatusCode.BadRequest)
                throw new UnauthorizedAccessException();
        }

        public async Task<WorkItem> GetWorkItem(string id) // Get work item and parse JSON into model
        {
            WorkItem workItem = new();
            var workItemResponse = await _httpClient.GetAsync($"wit/workitems/{id}?api-version=6.0");
            string rawResponse = await workItemResponse.Content.ReadAsStringAsync();
            if (!workItemResponse.IsSuccessStatusCode)
            {
                throw (workItemResponse.StatusCode == HttpStatusCode.Unauthorized || workItemResponse.StatusCode == HttpStatusCode.Forbidden)
                    ? new UnauthorizedAccessException(rawResponse) : new WorkItemClientException(rawResponse);
            }

            var doc = JsonDocument.Parse(rawResponse);
            var fields = doc.RootElement.GetProperty("fields");

            workItem.Id = id;
            fields.TryGetProperty("System.Title", out JsonElement titleElement);
            workItem.Title = GetJsonValue(titleElement);
            fields.TryGetProperty("System.Tags", out JsonElement tagsElement);
            workItem.Tags = GetJsonValue(tagsElement).Split(";", StringSplitOptions.RemoveEmptyEntries).Select(tag => tag?.Trim()) ?? Array.Empty<string>();

            if (_credential.ShowDescription)
            {
                fields.TryGetProperty("System.Description", out JsonElement descriptionElement);
                workItem.Description = GetJsonValue(descriptionElement);
                fields.TryGetProperty("Microsoft.VSTS.Common.AcceptanceCriteria", out JsonElement criteriaElement);
                workItem.AcceptanceCriteria = GetJsonValue(criteriaElement);
            }

            return workItem;
        }

        public async Task SizeWorkItem(string id, int size) // Update work item in DevOps
        {
            WorkItem workItem = await GetWorkItem(id); // Get latest work item in case there were changes

            List<string> tags = _tagsToAdd.ToList();
            tags.AddRange(workItem.Tags?.Where(tag => !_tagsToRemove.Contains(tag)) ?? new List<string>());
            string tagList = string.Join(';', tags);

            var request = new List<object>() // Always update size and conditionally update tags and state
            {
                new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.Effort", value = $"{size}"},
            };

            if (!string.IsNullOrWhiteSpace(_newState))
                request.Add(new { op = "add", path = "/fields/System.State", value = _newState });

            if (_tagsToAdd?.Any() == true || _tagsToRemove?.Any() == true)
                request.Add(new { op = "replace", path = "/fields/System.Tags", value = tagList });

            string requestBody = JsonSerializer.Serialize(request);
            var workItemResponse = await _httpClient.PatchAsync($"wit/workitems/{id}?api-version=6.0", new StringContent(requestBody, Encoding.UTF8, "application/json-patch+json"));
            string rawResponse = await workItemResponse.Content.ReadAsStringAsync();

            if (!workItemResponse.IsSuccessStatusCode)
            {
                throw (workItemResponse.StatusCode == HttpStatusCode.Unauthorized || workItemResponse.StatusCode == HttpStatusCode.Forbidden)
                    ? new UnauthorizedAccessException(rawResponse) : new WorkItemClientException(rawResponse);
            }
        }

        private string GetJsonValue(JsonElement element) // Helper to get Json Value from JsonElement
        {
            if (element.ValueKind == JsonValueKind.String || element.ValueKind == JsonValueKind.Null)
                return element.GetString() ?? string.Empty;
            else
                return string.Empty;
        }
    }
}

