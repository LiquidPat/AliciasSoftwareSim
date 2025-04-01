namespace AliciasSoftwareSim.Model;

public class GameProject
{
    public string Title { get; set; }
    public double Progress { get; set; }  // 0–100%
    public bool IsCompleted { get; set; }
    public bool IsReleased { get; set; }
    public double Revenue { get; set; }
    
    public double BudgetUsed { get; set; }
    
    public Team AssignedTeam { get; set; }



}
