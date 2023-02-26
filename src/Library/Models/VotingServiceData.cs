namespace ScrumStorySizer.Library.Models;

public class VotingServiceData
{
    public WorkItem WorkItem { get; set; } = new WorkItem();

    public List<SizeVote> StorySizeVotes { get; set; } = new List<SizeVote>();

    public bool ShowVotes { get; set; }

    public int ConnectedClients { get; set; }

    public int TimeLeft { get; set; } = 0;
}