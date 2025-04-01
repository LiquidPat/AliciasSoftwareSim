
using AliciasSoftwareSim.Model;

public class Company
{
    public string Name { get; set; }
    public double Budget { get; set; }
    public List<Employee> Employees { get; set; } = new();
    public List<GameProject> Projects { get; set; } = new();
    public List<Team> Teams { get; set; } = new();

}

