namespace AliciasSoftwareSim.Model;

public class Team
{
    public string Name { get; set; }
    public List<Employee> Members { get; set; } = new();
}