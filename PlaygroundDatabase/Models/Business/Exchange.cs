namespace PlaygroundDatabase.Models.Business;

/// <summary>
/// Model representing an exchange between two players
/// </summary>
public class Exchange
{
    public int Id { get; set; }
    public string RequestOpener { get; set; } = string.Empty;
    public string RequestFollower { get; set; } = string.Empty;
    public string OpenerCard { get; set; } = string.Empty;
    public string FollowerCard { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    public override string ToString()
    {
        return $"ID: {Id} | Opener: {RequestOpener} | Follower: {RequestFollower} | Cards: {OpenerCard} <-> {FollowerCard} | Date: {Date:yyyy-MM-dd HH:mm:ss}";
    }
}
