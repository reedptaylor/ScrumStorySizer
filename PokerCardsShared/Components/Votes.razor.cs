using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PokerCardsShared.Enums;
using PokerCardsShared.Models;

namespace PokerCardsShared.Components
{
    public partial class Votes
    {
        [Parameter] public List<SizeVote> SizeVotes { get; set; }

        [Inject] protected IJSRuntime JSRuntime { get; set; }

        private double avgStoryPoints;
        private StorySize avgStorySize;
        private List<List<SizeVote>> groupList;

        protected override async Task OnInitializedAsync()
        {
            avgStoryPoints = SizeVotes.Select(size => size.Size).Cast<int>().Sum() / (double)SizeVotes.Count;
            IEnumerable<int> storySizes = ((StorySize[])Enum.GetValues(typeof(StorySize))).Cast<int>();
            int closest = storySizes.Aggregate((x, y) => Math.Abs(x - avgStoryPoints) < Math.Abs(y - avgStoryPoints) ? x : y);
            avgStorySize = Enum.Parse<StorySize>(closest.ToString());
            groupList = SizeVotes.OrderByDescending(item => item.Size).GroupBy(item => item.Size).Select(grp => grp.ToList()).ToList();
            if (groupList.Count() == 1 && groupList[0].Count > 0)
            {
                await JSRuntime.InvokeVoidAsync("confetti.start", 1000);
            }
        }
    }
}
