using System.Collections.ObjectModel;
using System.Windows.Input;
using PlayWithSQLITE.Models.DTOs;
using PlayWithSQLITE.Services;

namespace PlayWithSQLITE.ViewModels;

public class ReportsViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private string _statusMessage = string.Empty;

    public ObservableCollection<CourseGradeSummary> CourseSummaries { get; } = new();
    public ObservableCollection<GradeLevelCount> GradeDistributions { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadReportsCommand { get; }
    public ICommand SeedDataCommand { get; }
    public ICommand ClearDataCommand { get; }

    public ReportsViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "דוחות ושאילתות מתקדמות (JOIN & GROUP BY)";

        LoadReportsCommand = new Command(async () => await LoadReportsAsync());
        SeedDataCommand = new Command(async () => await SeedDataAsync());
        ClearDataCommand = new Command(async () => await ClearDataAsync());
    }

    public async Task LoadReportsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusMessage = "טוען דוחות מרובי-טבלאות מהמסד...";

            var summaries = await _databaseService.GetCourseGradeSummariesAsync();
            CourseSummaries.Clear();
            foreach (var s in summaries)
            {
                CourseSummaries.Add(s);
            }

            var distributions = await _databaseService.GetStudentDistributionByGradeLevelAsync();
            GradeDistributions.Clear();
            foreach (var d in distributions)
            {
                GradeDistributions.Add(d);
            }

            StatusMessage = $"נטענו {CourseSummaries.Count} דוחות קורסים ו-{GradeDistributions.Count} שכבות גיל.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"שגיאה בטעינת דוחות: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SeedDataAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            StatusMessage = "מייצר נתוני הדגמה (מורים, קורסים, תלמידים ורישומים)...";
            await _databaseService.SeedSampleDataAsync();
            StatusMessage = "נתוני ההדגמה נוצרו בהצלחה!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"שגיאה ביצירת נתוני הדגמה: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }

        await LoadReportsAsync();
    }

    private async Task ClearDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "מוחק את כל הנתונים ממסד הנתונים...";
            await _databaseService.ClearAllDataAsync();
            CourseSummaries.Clear();
            GradeDistributions.Clear();
            StatusMessage = "המסד רוקן בהצלחה.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"שגיאה במחיקת נתונים: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
