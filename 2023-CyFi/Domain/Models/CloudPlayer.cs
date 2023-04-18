namespace Domain.Models
{
    public class CloudPlayer
    {
        public string PlayerParticipantId { get; set; }
        public string GamePlayerId { get; set; }
        public long FinalScore { get; set; }
        public int Placement { get; set; }
        public string Seed { get; set; }
        public int MatchPoints { get; set; }
    }
}
