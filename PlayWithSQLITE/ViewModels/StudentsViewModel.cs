using System.Collections.ObjectModel;
using System.Windows.Input;
using PlayWithSQLITE.Models;
using PlayWithSQLITE.Models.DTOs;
using PlayWithSQLITE.Services;

namespace PlayWithSQLITE.ViewModels;

public class StudentsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private int _currentPage = 1;
    private int _pageSize = 5;
    private int _totalStudents;
    private Student? _selectedStudent;
    private int _studentTotalCredits;
    private string _newFirstName = string.Empty;
    private string _newLastName = string.Empty;
    private string _newEmail = string.Empty;
    private int _newGradeLevel = 10;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<StudentTranscriptItem> SelectedStudentTranscript { get; } = new();

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage == value) return;
            _currentPage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageInfo));
            ((Command)NextPageCommand).ChangeCanExecute();
            ((Command)PrevPageCommand).ChangeCanExecute();
        }
    }

    public int TotalStudents
    {
        get => _totalStudents;
        set
        {
            if (_totalStudents == value) return;
            _totalStudents = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(TotalPages));
            ((Command)NextPageCommand).ChangeCanExecute();
            ((Command)PrevPageCommand).ChangeCanExecute();
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalStudents / _pageSize));

    public string PageInfo => $"עמוד {CurrentPage} מתוך {TotalPages} (סה\"כ {TotalStudents} תלמידים)";

    public Student? SelectedStudent
    {
        get => _selectedStudent;
        set
        {
            if (_selectedStudent == value) return;
            _selectedStudent = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedStudent));
            if (_selectedStudent != null)
            {
                _ = LoadTranscriptForStudentAsync(_selectedStudent.Id);
            }
        }
    }

    public bool HasSelectedStudent => SelectedStudent != null;

    public int StudentTotalCredits
    {
        get => _studentTotalCredits;
        set
        {
            if (_studentTotalCredits == value) return;
            _studentTotalCredits = value;
            OnPropertyChanged();
        }
    }

    public string NewFirstName
    {
        get => _newFirstName;
        set
        {
            if (_newFirstName == value) return;
            _newFirstName = value;
            OnPropertyChanged();
            ((Command)AddStudentCommand).ChangeCanExecute();
        }
    }

    public string NewLastName
    {
        get => _newLastName;
        set
        {
            if (_newLastName == value) return;
            _newLastName = value;
            OnPropertyChanged();
            ((Command)AddStudentCommand).ChangeCanExecute();
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
            ((Command)AddStudentCommand).ChangeCanExecute();
        }
    }

    public int NewGradeLevel
    {
        get => _newGradeLevel;
        set
        {
            if (_newGradeLevel == value) return;
            _newGradeLevel = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadStudentsCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }

    public StudentsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "ניהול תלמידים ודפדוף";

        LoadStudentsCommand = new Command(async () => await LoadStudentsAsync());
        NextPageCommand = new Command(async () => await NextPageAsync(), () => CurrentPage < TotalPages);
        PrevPageCommand = new Command(async () => await PrevPageAsync(), () => CurrentPage > 1);
        AddStudentCommand = new Command(async () => await AddStudentAsync(), CanAddStudent);
        DeleteStudentCommand = new Command<Student>(async (student) => await DeleteStudentAsync(student));
    }

    private bool CanAddStudent()
    {
        return !string.IsNullOrWhiteSpace(NewFirstName) &&
               !string.IsNullOrWhiteSpace(NewLastName) &&
               !string.IsNullOrWhiteSpace(NewEmail);
    }

    public async Task LoadStudentsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            TotalStudents = await _databaseService.GetStudentsCountAsync();

            // דפדוף בעזרת SKIP ו-TAKE
            int skip = (CurrentPage - 1) * _pageSize;
            var list = await _databaseService.GetStudentsAsync(skip, _pageSize);

            Students.Clear();
            foreach (var item in list)
            {
                Students.Add(item);
            }

            if (SelectedStudent != null)
            {
                await LoadTranscriptForStudentAsync(SelectedStudent.Id);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadStudentsAsync();
        }
    }

    private async Task PrevPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadStudentsAsync();
        }
    }

    private async Task AddStudentAsync()
    {
        if (!CanAddStudent()) return;

        var student = new Student
        {
            FirstName = NewFirstName.Trim(),
            LastName = NewLastName.Trim(),
            Email = NewEmail.Trim(),
            GradeLevel = NewGradeLevel
        };

        await _databaseService.SaveStudentAsync(student);

        NewFirstName = string.Empty;
        NewLastName = string.Empty;
        NewEmail = string.Empty;

        await LoadStudentsAsync();
    }

    private async Task DeleteStudentAsync(Student? student)
    {
        if (student == null) return;

        await _databaseService.DeleteStudentAsync(student);
        if (SelectedStudent?.Id == student.Id)
        {
            SelectedStudent = null;
            SelectedStudentTranscript.Clear();
            StudentTotalCredits = 0;
        }

        await LoadStudentsAsync();
    }

    private async Task LoadTranscriptForStudentAsync(int studentId)
    {
        SelectedStudentTranscript.Clear();
        var items = await _databaseService.GetStudentTranscriptAsync(studentId);
        foreach (var item in items)
        {
            SelectedStudentTranscript.Add(item);
        }

        StudentTotalCredits = await _databaseService.GetTotalCreditsForStudentAsync(studentId);
    }
}
