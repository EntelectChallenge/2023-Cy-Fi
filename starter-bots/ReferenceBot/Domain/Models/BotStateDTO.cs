namespace Domain.Models
{
  public class BotStateDTO
  {
    public int CurrentLevel { get; set; }
    public string ConnectionId { get; set; }
    public int Collected { get; set; }
    public string ElapsedTime { get; set; }
    public int[][] HeroWindow { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
  }
}