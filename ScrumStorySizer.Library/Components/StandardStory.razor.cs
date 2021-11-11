using Microsoft.AspNetCore.Components;
using ScrumStorySizer.Library.Models;

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

        private void ReconcileStoryName()
        {
            _serverStoryName = PokerVote.WorkItem?.Title;
            if (!_titleFocus)
                _storyName = _serverStoryName;
            OnUpdate();
        }

        protected override void OnInitialized()
        {
            PokerVote.OnChange += ReconcileStoryName;
            _storyName = PokerVote.WorkItem?.Title;
            _serverStoryName = PokerVote.WorkItem?.Title;
        }

        public override void Dispose()
        {
            base.Dispose();
            PokerVote.OnChange -= ReconcileStoryName;
        }
    }
}
