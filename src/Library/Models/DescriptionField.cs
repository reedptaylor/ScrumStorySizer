using Microsoft.AspNetCore.Components;

namespace ScrumStorySizer.Library.Models
{
    public class DescriptionField
    {
        public string DisplayName { get; set; }
        public string ApiName { get; set; }
        public string Value { get; set; }
    }

    public class MarkupDescriptionField : DescriptionField
    {
        public MarkupDescriptionField(DescriptionField field)
        {
            if (field != null)
            {
                DisplayName = field.DisplayName;
                ApiName = field.ApiName;
                Value = field.Value;
            }
            MarkupValue = field?.Value.SanitizeHTML().ConvertToMarkupString() ?? new();
        }

        public MarkupString MarkupValue { get; set; }
    }
}