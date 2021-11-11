using ScrumStorySizer.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace ScrumStorySizer.Library.Services
{
    public class DevOpsClient : IWorkItemClient
    {
        private readonly HttpClient _httpClient;
        private readonly DevOpsCredential _credential;

        public DevOpsClient(HttpClient httpClient, NavigationManager navigationManager, DevOpsCredential credential)
        {
            _httpClient = httpClient;
            _credential = credential;

            _httpClient.BaseAddress = new Uri($"{navigationManager.BaseUri}devops/{credential.Organization}/{credential.Project}/_apis/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credential.BasicAuth);
        }

        public async Task TestAuthentication()
        {
            string requestUri = $"wit/workitems?ids=0&api-version=6.0";
            var workItemResponse = await _httpClient.GetAsync(requestUri);
            if (!workItemResponse.IsSuccessStatusCode && workItemResponse.StatusCode != HttpStatusCode.BadRequest)
                throw new UnauthorizedAccessException();
        }

        public async Task<WorkItem> GetWorkItem(string id)
        {
            WorkItem workItem = new();
            string requestUri = $"wit/workitems/{id}?api-version=6.0";
            var workItemResponse = await _httpClient.GetAsync(requestUri);
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
            fields.TryGetProperty("System.Description", out JsonElement descriptionElement);
            workItem.Description = GetJsonValue(descriptionElement);
            fields.TryGetProperty("Microsoft.VSTS.Common.AcceptanceCriteria", out JsonElement criteriaElement);
            workItem.AcceptanceCriteria = GetJsonValue(criteriaElement);
            fields.TryGetProperty("System.Tags", out JsonElement tagsElement);
            workItem.Tags = GetJsonValue(tagsElement).Split(";") ?? Array.Empty<string>();

            return workItem;
        }

        public async Task SizeWorkItem(string id, int size)
        {
            WorkItem workItem = await GetWorkItem(id);
            List<string> tags = new List<string>() { "Planning" };
            tags.AddRange(workItem?.Tags?.Where(tag => tag != "Ready2Groom"));
            string tagList = string.Join(';', tags);

            string requestUri = $"wit/workitems/{id}?api-version=6.0";
            var request = new[]
            {
                new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.Effort", value = $"{size}"},
                new { op = "add", path = "/fields/System.State", value = "Approved"},
                new { op = "update", path = "/fields/System.Tags", value = tagList},
            };
            string requestBody = JsonSerializer.Serialize(request);

            var workItemResponse = await _httpClient.PatchAsync(requestUri, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            string rawResponse = await workItemResponse.Content.ReadAsStringAsync();
            if (!workItemResponse.IsSuccessStatusCode)
            {
                throw (workItemResponse.StatusCode == HttpStatusCode.Unauthorized || workItemResponse.StatusCode == HttpStatusCode.Forbidden)
                    ? new UnauthorizedAccessException(rawResponse) : new WorkItemClientException(rawResponse);
            }
        }

        private string GetJsonValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String || element.ValueKind == JsonValueKind.Null)
                return element.GetString() ?? string.Empty;
            else
                return string.Empty;
        }
    }
}

