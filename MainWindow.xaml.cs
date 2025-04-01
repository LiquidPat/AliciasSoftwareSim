using System.Windows;
using AliciasSoftwareSim.Model;

namespace AliciasSoftwareSim;

public partial class MainWindow : Window
{
    private Company _company;
    private GameTimer _gameTimer;
    private int _dayCounter = 0;
    private List<Employee> _availableEmployees = new();

    
    public enum SkillCategory
    {
        Entwicklung,
        Dienstleistungen
    }

    public enum SkillType
    {
        Programmieren,
        Design,
        Marketing,
        Recht,
        Teamleitung
    }

    
    
    
    public enum GameSpeed
    {
        Paused,
        Normal,
        Fast,
        Ultra
    }

    
    
    public MainWindow()
    {
        _gameTimer = new GameTimer();
        _gameTimer.OnTick += GameTick;
        InitializeComponent();

        Loaded += (_, _) =>
        {
            Task.Delay(2000).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() => SplashGrid.Visibility = Visibility.Collapsed);
            });
        };
        

        _gameTimer.Start();
        
        
        _availableEmployees = new List<Employee>
        {
            new Employee
            {
                Name = "Anna Dev",
                Salary = 500,
                Skills = new()
                {
                    new Skill { Type = SkillType.Programmieren, Level = 85 },
                    new Skill { Type = SkillType.Design, Level = 40 }
                }
            }
        };

        EmployeeList.ItemsSource = _availableEmployees;

        
    }


    private void GameTick()
    {
        
        if (_company == null) return;
        
        _dayCounter++;

        if (_company != null)
        {
            double gehaltSumme = _company.Employees.Sum(e => e.Salary);
            _company.Budget -= gehaltSumme;

            Dispatcher.Invoke(() =>
            {
                OutputText.Text = $"Tag {_dayCounter}\n" +
                                  $"Firma: {_company.Name}\n" +
                                  $"Budget: {_company.Budget:N0}€\n" +
                                  $"Mitarbeiter: {_company.Employees.Count}";
            });
        }

        foreach (var project in _company.Projects)
        {
            if (project.IsCompleted || project.AssignedTeam == null) continue;

            double productivity = project.AssignedTeam.Members.Sum(member =>
            {
                int prog = member.GetSkillLevel(SkillType.Programmieren);
                int design = member.GetSkillLevel(SkillType.Design);
                return (prog + design) / 2.0 * 0.01;
            });
            
            project.Progress += productivity;

            if (project.Progress >= 100)
            {
                project.Progress = 100;
                project.IsCompleted = true;
                MessageBox.Show($"Projekt '{project.Title}' wurde fertiggestellt!");
            }
        }

        Dispatcher.Invoke(() =>
        {
            OutputText.Text = $"Tag {_dayCounter}\n" +
                              $"Firma: {_company.Name}\n" +
                              $"Budget: {_company.Budget:N0}€\n" +
                              $"Mitarbeiter: {_company.Employees.Count}";

            RefreshProjectList();
        });

        foreach (var project in _company.Projects)
        {
            if (project.IsCompleted && !project.IsReleased)
            {
                ReleaseProject(project);
            }
        }
        
        
    }

    private void HireEmployee_Click(object sender, RoutedEventArgs e)
    {
        
        if (_company == null)
        {
            MessageBox.Show("Bitte starte das Spiel zuerst.");
            return;
        }
        
        if (EmployeeList.SelectedItem is Employee selected)
        {
            _company.Employees.Add(selected);
            _availableEmployees.Remove(selected);
            EmployeeList.ItemsSource = null;
            EmployeeList.ItemsSource = _availableEmployees;

            MessageBox.Show($"{selected.Name} wurde eingestellt!");
        }
        else
        {
            MessageBox.Show("Bitte einen Mitarbeiter auswählen.");
        }
        
        RefreshHiredEmployees();

    }

    
    private void Pause_Click(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Paused);
    private void Speed1_Click(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Normal);
    private void Speed2_Click(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Fast);
    private void Speed4_Click(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Ultra);
    private void Pause_Checked(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Paused);
    private void Speed1_Checked(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Normal);
    private void Speed2_Checked(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Fast);
    private void Speed4_Checked(object sender, RoutedEventArgs e) => _gameTimer.SetSpeed(GameSpeed.Ultra);

    
    
    
    private void StartGame_Click(object sender, RoutedEventArgs e)
    {
        var companyName = CompanyNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(companyName))
        {
            MessageBox.Show("Bitte gib einen Firmennamen ein.");
            return;
        }

        _company = new Company
        {
            Name = companyName,
            Budget = 100_000
        };

        RefreshTeams();
        RefreshAvailableEmployees();
        RefreshHiredEmployees();

        
        OutputText.Text = $"Firma '{_company.Name}' gegründet!\nStartbudget: {_company.Budget}€";

    }

    private void CreateProject_Click(object sender, RoutedEventArgs e)
    {
        string title = ProjectNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Bitte Projektnamen eingeben.");
            return;
        }
        
        RefreshProjectList();
        ProjectNameBox.Text = "";
        
        if (ProjectTeamBox.SelectedItem is not Team selectedTeam)
        {
            MessageBox.Show("Bitte ein Team auswählen.");
            return;
        }

        var project = new GameProject
        {
            Title = title,
            Progress = 0,
            AssignedTeam = selectedTeam
        };

        _company.Projects.Add(project);
        RefreshProjectList();
        ProjectNameBox.Text = "";

    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_company == null) return;

        SaveService.SaveGame(_company);
        MessageBox.Show("Spiel gespeichert.");
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var loaded = SaveService.LoadGame();
        if (loaded != null)
        {
            _company = loaded;
            RefreshProjectList();
            MessageBox.Show($"Spiel von '{_company.Name}' geladen.");
        }
        else
        {
            MessageBox.Show("Kein Speicherstand gefunden.");
        }
    }

    private void CreateTeam_Click(object sender, RoutedEventArgs e)
    {

        if (_company == null)
        {
            MessageBox.Show("Bitte starte das Spiel zuerst.");
            return;
        }
        
        string teamName = TeamNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(teamName))
        {
            MessageBox.Show("Bitte einen Teamnamen eingeben.");
            return;
        }

        if (_company.Teams.Any(t => t.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Teamname bereits vergeben.");
            return;
        }

        var team = new Team { Name = teamName };
        _company.Teams.Add(team);

        RefreshTeams();
        TeamNameBox.Text = "";
    }
    
    private void RefreshHiredEmployees()
    {
        HiredEmployeesList.ItemsSource = null;
        HiredEmployeesList.ItemsSource = _company?.Employees;
    }


    
    private void RefreshProjectList()
    {
        ProjectList.ItemsSource = null;
        ProjectList.ItemsSource = _company.Projects;
    }
    
    private void ReleaseProject(GameProject project)
    {
        if (project.IsReleased || !project.IsCompleted) return;

        double avgSkill = _company.Employees.Count == 0
            ? 0
            : _company.Employees.Average(e =>
                (e.GetSkillLevel(SkillType.Programmieren) + e.GetSkillLevel(SkillType.Design)) / 2.0);

        var rand = new Random();
        double quality = avgSkill + rand.NextDouble() * 20 - 10; // +-10 Zufall
        quality = Math.Clamp(quality, 0, 100);

        double revenue = quality * 1000 + rand.Next(10_000); // grob geschätzt

        project.Revenue = revenue;
        project.IsReleased = true;
        _company.Budget += revenue;

        MessageBox.Show($"'{project.Title}' wurde veröffentlicht!\nQualität: {quality:F1}\nEinnahmen: {revenue:N0}€");
    }
    
    
    public class Skill
    {
        public SkillType Type { get; set; }
        public int Level { get; set; }  // 0–100

        public SkillCategory Category =>
            Type switch
            {
                SkillType.Programmieren => SkillCategory.Entwicklung,
                SkillType.Design => SkillCategory.Entwicklung,
                SkillType.Marketing => SkillCategory.Dienstleistungen,
                SkillType.Recht => SkillCategory.Dienstleistungen,
                SkillType.Teamleitung => SkillCategory.Dienstleistungen,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    
    private void RefreshTeams()
    {
        TeamSelectBox.ItemsSource = null;
        TeamSelectBox.ItemsSource = _company.Teams;
        ProjectTeamBox.ItemsSource = null;
        ProjectTeamBox.ItemsSource = _company.Teams;
    }

    private void RefreshAvailableEmployees()
    {
        var assigned = _company.Teams.SelectMany(t => t.Members).ToList();
        var available = _company.Employees.Except(assigned).ToList();

        AvailableEmployeesBox.ItemsSource = null;
        AvailableEmployeesBox.ItemsSource = available;
    }

    private void AssignEmployeeToTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamSelectBox.SelectedItem is not Team team)
        {
            MessageBox.Show("Bitte ein Team auswählen.");
            return;
        }

        if (AvailableEmployeesBox.SelectedItem is not Employee employee)
        {
            MessageBox.Show("Bitte einen Mitarbeiter auswählen.");
            return;
        }

        if (!team.Members.Contains(employee))
            team.Members.Add(employee);

        RefreshAvailableEmployees();
        MessageBox.Show($"{employee.Name} wurde dem Team '{team.Name}' zugewiesen.");
    }

    
    private void AssignHiredToTeam_Click(object sender, RoutedEventArgs e)
    {
        if (HiredEmployeesList.SelectedItem is not Employee selected) return;
        if (TeamSelectBox.SelectedItem is not Team team) return;

        if (!team.Members.Contains(selected))
        {
            team.Members.Add(selected);
            MessageBox.Show($"{selected.Name} wurde dem Team '{team.Name}' zugewiesen.");
        }

        RefreshAvailableEmployees();
    }

    
    private void GenerateNewApplicants_Click(object sender, RoutedEventArgs e)
    {
        var rnd = new Random();
        _availableEmployees = new List<Employee>
        {
            CreateRandomEmployee("Max Muster", rnd),
            CreateRandomEmployee("Lisa Logic", rnd),
            CreateRandomEmployee("Tim Team", rnd)
        };

        EmployeeList.ItemsSource = null;
        EmployeeList.ItemsSource = _availableEmployees;
    }

    private Employee CreateRandomEmployee(string name, Random rnd)
    {
        var skills = Enum.GetValues(typeof(SkillType)).Cast<SkillType>()
            .Select(type => new Skill
            {
                Type = type,
                Level = rnd.Next(30, 100)
            }).ToList();

        return new Employee
        {
            Name = name,
            Salary = rnd.Next(700, 1200),
            Skills = skills
        };
    }

    private void FireEmployee_Click(object sender, RoutedEventArgs e)
    {
        if (HiredEmployeesList.SelectedItem is not Employee selected) return;

        // Entferne aus Teams
        foreach (var team in _company.Teams)
        {
            team.Members.Remove(selected);
        }

        // Entferne aus Company
        _company.Employees.Remove(selected);

        RefreshHiredEmployees();
        RefreshAvailableEmployees();
        MessageBox.Show($"{selected.Name} wurde entlassen.");
    }

    private void ClearTeam_Click(object sender, RoutedEventArgs e)
    {
        if (TeamSelectBox.SelectedItem is not Team team) return;

        team.Members.Clear();
        RefreshAvailableEmployees();
        MessageBox.Show($"Team '{team.Name}' wurde geleert.");
    }

    
}
