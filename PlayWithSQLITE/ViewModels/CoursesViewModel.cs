using System.Collections.ObjectModel;
using System.Windows.Input;
using PlayWithSQLITE.Models;
using PlayWithSQLITE.Services;

namespace PlayWithSQLITE.ViewModels;

public class CoursesViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private string _newCourseTitle = string.Empty;
    private int _newCourseCredits = 3;
    private string _newCourseDescription = string.Empty;
    private Teacher? _selectedTeacher;
    private Course? _selectedCourse;

    public ObservableCollection<Course> Courses { get; } = new();
    public ObservableCollection<Teacher> Teachers { get; } = new();

    public string NewCourseTitle
    {
        get => _newCourseTitle;
        set
        {
            if (_newCourseTitle == value) return;
            _newCourseTitle = value;
            OnPropertyChanged();
            ((Command)AddCourseCommand).ChangeCanExecute();
        }
    }

    public int NewCourseCredits
    {
        get => _newCourseCredits;
        set
        {
            if (_newCourseCredits == value) return;
            _newCourseCredits = value;
            OnPropertyChanged();
        }
    }

    public string NewCourseDescription
    {
        get => _newCourseDescription;
        set
        {
            if (_newCourseDescription == value) return;
            _newCourseDescription = value;
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
            ((Command)AddCourseCommand).ChangeCanExecute();
        }
    }

    public Course? SelectedCourse
    {
        get => _selectedCourse;
        set
        {
            if (_selectedCourse == value) return;
            _selectedCourse = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCoursesCommand { get; }
    public ICommand AddCourseCommand { get; }
    public ICommand DeleteCourseCommand { get; }

    public CoursesViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "ניהול קורסים ומורים מקושרים";

        LoadCoursesCommand = new Command(async () => await LoadCoursesAsync());
        AddCourseCommand = new Command(async () => await AddCourseAsync(), CanAddCourse);
        DeleteCourseCommand = new Command<Course>(async (course) => await DeleteCourseAsync(course));
    }

    private bool CanAddCourse()
    {
        return !string.IsNullOrWhiteSpace(NewCourseTitle) && SelectedTeacher != null;
    }

    public async Task LoadCoursesAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            var teachersList = await _databaseService.GetTeachersAsync();
            Teachers.Clear();
            foreach (var t in teachersList)
            {
                Teachers.Add(t);
            }

            var coursesList = await _databaseService.GetCoursesAsync(includeRelations: true);
            Courses.Clear();
            foreach (var c in coursesList)
            {
                Courses.Add(c);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddCourseAsync()
    {
        if (!CanAddCourse()) return;

        var course = new Course
        {
            Title = NewCourseTitle.Trim(),
            Credits = NewCourseCredits,
            Description = NewCourseDescription.Trim(),
            TeacherId = SelectedTeacher!.Id
        };

        await _databaseService.SaveCourseAsync(course);

        NewCourseTitle = string.Empty;
        NewCourseDescription = string.Empty;

        await LoadCoursesAsync();
    }

    private async Task DeleteCourseAsync(Course? course)
    {
        if (course == null) return;

        await _databaseService.DeleteCourseAsync(course);
        await LoadCoursesAsync();
    }
}
