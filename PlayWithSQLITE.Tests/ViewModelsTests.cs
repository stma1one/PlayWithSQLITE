using PlayWithSQLITE.Services;
using PlayWithSQLITE.ViewModels;
using Xunit;

namespace PlayWithSQLITE.Tests;

public class ViewModelsTests : IAsyncLifetime
{
    private readonly string _testDbPath;
    private readonly DatabaseService _databaseService;

    public ViewModelsTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"SchoolLms_VM_Test_{Guid.NewGuid():N}.db3");
        _databaseService = new DatabaseService(_testDbPath);
    }

    public async ValueTask InitializeAsync()
    {
        await _databaseService.SeedSampleDataAsync();
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }
        catch { }
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task TeachersViewModel_LoadTeachersAsync_PopulatesTeachersWithCourses()
    {
        var vm = new TeachersViewModel(_databaseService);

        Assert.False(vm.AddTeacherCommand.CanExecute(null));
        vm.NewFullName = "מורה חדש";
        vm.NewEmail = "new@school.org";
        Assert.True(vm.AddTeacherCommand.CanExecute(null));

        await vm.LoadTeachersAsync();

        Assert.True(vm.Teachers.Count >= 3);
        Assert.Contains(vm.Teachers, t => t.Courses.Count > 0);
    }

    [Fact]
    public async Task CoursesViewModel_LoadCoursesAsync_PopulatesTeachersAndCoursesWithRelations()
    {
        var vm = new CoursesViewModel(_databaseService);

        await vm.LoadCoursesAsync();

        Assert.NotEmpty(vm.Teachers);
        Assert.NotEmpty(vm.Courses);
        Assert.All(vm.Courses, c => Assert.NotNull(c.Teacher));

        Assert.False(vm.AddCourseCommand.CanExecute(null));
        vm.NewCourseTitle = "קורס חדש";
        vm.SelectedTeacher = vm.Teachers[0];
        Assert.True(vm.AddCourseCommand.CanExecute(null));
    }

    [Fact]
    public async Task StudentsViewModel_LoadStudentsAsync_HandlesPaginationAndCanExecute()
    {
        var vm = new StudentsViewModel(_databaseService);

        await vm.LoadStudentsAsync();

        Assert.True(vm.TotalStudents >= 6);
        Assert.Equal(5, vm.Students.Count);
        Assert.Equal(1, vm.CurrentPage);
        Assert.False(vm.PrevPageCommand.CanExecute(null));
        Assert.True(vm.NextPageCommand.CanExecute(null));
    }

    [Fact]
    public async Task ReportsViewModel_LoadReportsAsync_PopulatesSummariesAndDistributions()
    {
        var vm = new ReportsViewModel(_databaseService);

        await vm.LoadReportsAsync();

        Assert.NotEmpty(vm.CourseSummaries);
        Assert.NotEmpty(vm.GradeDistributions);
        Assert.Contains("נטענו", vm.StatusMessage);
    }
}
