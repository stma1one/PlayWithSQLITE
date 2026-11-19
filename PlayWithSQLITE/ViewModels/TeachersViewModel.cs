using System.Collections.ObjectModel;
using System.Windows.Input;
using PlayWithSQLITE.Models;
using PlayWithSQLITE.Services;

namespace PlayWithSQLITE.ViewModels;

public class TeachersViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private string _newFullName = string.Empty;
    private string _newEmail = string.Empty;
    private string _newDepartment = "מדעי המחשב";
    private Teacher? _selectedTeacher;

    public ObservableCollection<Teacher> Teachers { get; } = new();

    public string NewFullName
    {
        get => _newFullName;
        set
        {
            if (_newFullName == value) return;
            _newFullName = value;
            OnPropertyChanged();
            ((Command)AddTeacherCommand).ChangeCanExecute();
        }
    }

    public string NewEmail
    {
        get => _newEmail;
        set
        {
            if (_newEmail == value) return;
            _newEmail = value;
            OnPropertyChanged();
            ((Command)AddTeacherCommand).ChangeCanExecute();
        }
    }

    public string NewDepartment
    {
        get => _newDepartment;
        set
        {
            if (_newDepartment == value) return;
            _newDepartment = value;
            OnPropertyChanged();
        }
    }

    public Teacher? SelectedTeacher
    {
        get => _selectedTeacher;
        set
        {
            if (_selectedTeacher == value) return;
            _selectedTeacher = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadTeachersCommand { get; }
    public ICommand AddTeacherCommand { get; }
    public ICommand DeleteTeacherCommand { get; }

    public TeachersViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "ניהול סגל מורים";

        LoadTeachersCommand = new Command(async () => await LoadTeachersAsync());
        AddTeacherCommand = new Command(async () => await AddTeacherAsync(), CanAddTeacher);
        DeleteTeacherCommand = new Command<Teacher>(async (teacher) => await DeleteTeacherAsync(teacher));
    }

    private bool CanAddTeacher()
    {
        return !string.IsNullOrWhiteSpace(NewFullName) && !string.IsNullOrWhiteSpace(NewEmail);
    }

    public async Task LoadTeachersAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            var list = await _databaseService.GetTeachersAsync();
            Teachers.Clear();
            foreach (var item in list)
            {
                // טעינת הקורסים עבור כל מורה להדגמת קשר 1:N
                var fullTeacher = await _databaseService.GetTeacherByIdAsync(item.Id, includeCourses: true);
                Teachers.Add(fullTeacher ?? item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddTeacherAsync()
    {
        if (!CanAddTeacher()) return;

        var teacher = new Teacher
        {
            FullName = NewFullName.Trim(),
            Email = NewEmail.Trim(),
            Department = NewDepartment.Trim()
        };

        await _databaseService.SaveTeacherAsync(teacher);

        NewFullName = string.Empty;
        NewEmail = string.Empty;

        await LoadTeachersAsync();
    }

    private async Task DeleteTeacherAsync(Teacher? teacher)
    {
        if (teacher == null) return;

        await _databaseService.DeleteTeacherAsync(teacher);
        await LoadTeachersAsync();
    }
}
