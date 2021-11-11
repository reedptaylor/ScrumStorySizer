using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScrumStorySizer.Library.Models;
using ScrumStorySizer.Library.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScrumStorySizer.Library.Components
{
    public partial class StandardStory : StoryBase
    {
        private string _storyName;
        private string _serverStoryName;
        private bool _titleFocus;

        private void UpdateStoryName(ChangeEventArgs e)
        {
            PokerVote.UpdateWorkItem(new WorkItem() { Title = e.Value.ToString() });
        }

        private void ReconcileStoryName() // TODO story syncing still doesnt work
        {
            _serverStoryName = PokerVote.WorkItem.Title;
            if (!_titleFocus)
                _storyName = _serverStoryName;
        }

        protected override void OnInitialized()
        {
            PokerVote.OnChange += ReconcileStoryName;
            _storyName = PokerVote.WorkItem.Title;
            _serverStoryName = PokerVote.WorkItem.Title;
        }

        public override void Dispose()
        {
            base.Dispose();
            PokerVote.OnChange -= ReconcileStoryName;
        }
    }
}
