using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Enums;
using ScrumStorySizer.Library.Models;

namespace ScrumStorySizer.Library.Components
{
    public partial class Votes // Shared Component to see votes after voting has finished
    {
        [Parameter] public List<SizeVote> SizeVotes { get; set; }
        [Parameter] public bool ShowConfetti { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }

        private double avgStoryPoints;
        private StorySize avgStorySize;
        private List<List<SizeVote>> groupList;

        protected override async Task OnInitializedAsync()
        {
            // Calculate average story points and story size
            avgStoryPoints = SizeVotes.Select(size => size.Size).Cast<int>().Sum() / (double)SizeVotes.Count;

            IEnumerable<int> storySizes = ((StorySize[])Enum.GetValues(typeof(StorySize))).Cast<int>();
            int closest = storySizes.Aggregate((x, y) => Math.Abs(x - avgStoryPoints) < Math.Abs(y - avgStoryPoints) ? x : y);
            avgStorySize = Enum.Parse<StorySize>(closest.ToString());

            // Set vote sizes to show (sizes that received votes)
            groupList = SizeVotes.OrderByDescending(item => item.Size).GroupBy(item => item.Size).Select(grp => grp.ToList()).ToList();

            await Task.Delay(100); // Delay to allow ShowConfetti to be set

            // Display confetti if all users voted the same size
            if (ShowConfetti && groupList.Count() == 1 && groupList[0].Count > 0)
            {
                await JSRuntime.InvokeVoidAsync("confetti.start", 1000);
            }
        }
    }
}
