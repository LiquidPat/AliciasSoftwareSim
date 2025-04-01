using AliciasSoftwareSim;

public class Employee
{
    public string Name { get; set; }
    public List<MainWindow.Skill> Skills { get; set; } = new();
    public double Salary { get; set; }
    
    public int GetSkillLevel(MainWindow.SkillType type)
    {
        return Skills.FirstOrDefault(s => s.Type == type)?.Level ?? 0;
    }

    public override string ToString()
    {
        string skillString = string.Join(", ", Skills.Select(s => $"{s.Type}: {s.Level}"));
        return $"{Name} - {skillString} - {Salary:N0}€/Tag";
    }
    
}