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

        private static readonly IEnumerable<DescriptionField> deafaultDescriptionFields = new List<DescriptionField>()
        {
            new DescriptionField() { DisplayName = "Description", ApiName = "System.Description" },
            new DescriptionField() { DisplayName = "Repro Steps", ApiName = "Microsoft.VSTS.TCM.ReproSteps" },
            new DescriptionField() { DisplayName = "Acceptance Criteria", ApiName = "Microsoft.VSTS.Common.AcceptanceCriteria" },
        };

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

        public async Task TestAuthentication(IEnumerable<string> tags, string state)
        {
            bool alreadyChecked = false;
            if (tags?.Any() == true)
            {
                var workItemResponse = await _httpClient.GetAsync($"wit/tags?api-version=6.0");
                if (workItemResponse.StatusCode != HttpStatusCode.OK)
                    throw new UnauthorizedAccessException();
                CheckTags(tags, JsonDocument.Parse(await workItemResponse.Content.ReadAsStringAsync()));
                alreadyChecked = true;
            }
            if (!string.IsNullOrWhiteSpace(state))
            {
                var workItemResponse = await _httpClient.GetAsync($"wit/workitemtypes/{Constants.DEFAULT_PBI_NAME}/states?api-version=6.0");
                if (workItemResponse.StatusCode != HttpStatusCode.OK)
                    throw new UnauthorizedAccessException();
                CheckState(state, JsonDocument.Parse(await workItemResponse.Content.ReadAsStringAsync()));
                alreadyChecked = true;
            }
            if (!alreadyChecked) // Do atleast 1 check to see if credentials are correct. Check if tags return any value.
            {
                var workItemResponse = await _httpClient.GetAsync($"wit/tags?api-version=6.0");
                if (workItemResponse.StatusCode != HttpStatusCode.OK)
                    throw new UnauthorizedAccessException();
            }
        }

        public async Task<WorkItem> GetWorkItem(string id, IEnumerable<DescriptionField> extraDescriptionFields = null) // Get work item and parse JSON into model
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
                workItem.DescriptionFields = deafaultDescriptionFields.Concat(extraDescriptionFields ?? new List<DescriptionField>()).ToList();
                foreach (DescriptionField descriptionField in workItem.DescriptionFields)
                {
                    fields.TryGetProperty(descriptionField.ApiName, out JsonElement element);
                    descriptionField.Value = GetJsonValue(element);
                }
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

        private void CheckState(string state, JsonDocument document)
        {
            IEnumerable<string> stateTypes = null;
            JsonElement valueElement = document.RootElement.GetProperty("value");
            if (valueElement.ValueKind == JsonValueKind.Array && valueElement.GetArrayLength() > 0)
            {
                stateTypes = valueElement
                    .EnumerateArray()
                    .Select(stateType => stateType.GetProperty("name").GetString());
            }

            if (stateTypes == null)
                throw new WorkItemClientException("Unable to verify allowed work item states.");

            if (!stateTypes?.Contains(state) ?? false)
                throw new WorkItemClientException($"State {state} is not valid for {Constants.DEFAULT_PBI_NAME} in this project.");
        }

        private void CheckTags(IEnumerable<string> tags, JsonDocument document)
        {
            IEnumerable<string> allowedTags = null;
            JsonElement valueElement = document.RootElement.GetProperty("value");
            if (valueElement.ValueKind == JsonValueKind.Array && valueElement.GetArrayLength() > 0)
            {
                allowedTags = valueElement
                    .EnumerateArray()
                    .Select(stateType => stateType.GetProperty("name").GetString());
            }

            if (allowedTags == null)
                throw new WorkItemClientException("Unable to verify allowed work item tags.");

            IEnumerable<string> forbiddenTags = tags.Except(allowedTags);

            if (forbiddenTags.Any())
                throw new WorkItemClientException($"Tag{(forbiddenTags.Count() > 1 ? "s" : string.Empty)} \"{string.Join(", ", forbiddenTags)}\" {(forbiddenTags.Count() > 1 ? "are" : "is")} not valid for this project.");
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

