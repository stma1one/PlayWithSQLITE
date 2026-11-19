using PlayWithSQLITE.Models;
using PlayWithSQLITE.Services;
using Xunit;

namespace PlayWithSQLITE.Tests;

public class DatabaseServiceTests : IAsyncLifetime
{
    private readonly string _testDbPath;
    private readonly DatabaseService _databaseService;

    public DatabaseServiceTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"SchoolLms_Test_{Guid.NewGuid():N}.db3");
        _databaseService = new DatabaseService(_testDbPath);
    }

    public async ValueTask InitializeAsync()
    {
        // ׳”׳׳×׳—׳•׳ ׳•׳™׳¦׳™׳¨׳× ׳”׳˜׳‘׳׳׳•׳× ׳׳×׳‘׳¦׳¢׳™׳ ׳׳•׳˜׳•׳׳˜׳™׳× ׳‘׳×׳•׳ DatabaseService (InitDBAsync)
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
    public async Task GetTeachersAsync_ReturnsSeededTeachers()
    {
        var teachers = await _databaseService.GetTeachersAsync();

        Assert.NotNull(teachers);
        Assert.True(teachers.Count >= 3);
        Assert.Contains(teachers, t => t.FullName == "׳™׳©׳¨׳׳ ׳™׳©׳¨׳׳׳™");
    }

    [Fact]
    public async Task GetCoursesAsync_WithRelations_LoadsTeacherDetails()
    {
        var courses = await _databaseService.GetCoursesAsync(includeRelations: true);

        Assert.NotNull(courses);
        Assert.NotEmpty(courses);
        var courseWithTeacher = courses.FirstOrDefault(c => c.Teacher != null);
        Assert.NotNull(courseWithTeacher);
        Assert.False(string.IsNullOrWhiteSpace(courseWithTeacher.Teacher?.FullName));
    }

    [Fact(Skip = "׳©׳׳‘ 4: ׳™׳•׳₪׳¢׳ ׳׳׳—׳¨ ׳׳™׳׳•׳© ׳—׳׳•׳§׳” ׳׳¢׳׳•׳“׳™׳ ׳¢׳ Skip ׳•-Take")]
    public async Task Pagination_SkipAndTake_ReturnsCorrectSlices()
    {
        int total = await _databaseService.GetStudentsCountAsync();
        Assert.True(total >= 6);

        var firstSlice = await _databaseService.GetStudentsAsync(skip: 0, take: 3);
        var secondSlice = await _databaseService.GetStudentsAsync(skip: 3, take: 3);

        Assert.Equal(3, firstSlice.Count);
        Assert.Equal(3, secondSlice.Count);
        Assert.NotEqual(firstSlice[0].Id, secondSlice[0].Id);
    }

    [Fact(Skip = "׳©׳׳‘ 4: ׳™׳•׳₪׳¢׳ ׳׳׳—׳¨ ׳׳™׳׳•׳© ׳©׳׳™׳׳×׳× ׳§׳™׳‘׳•׳¥ ׳•-JOIN ׳׳¡׳™׳›׳•׳ ׳¦׳™׳•׳ ׳™׳")]
    public async Task GetCourseGradeSummariesAsync_PerformsJoinAndGroupBy()
    {
        var summaries = await _databaseService.GetCourseGradeSummariesAsync();

        Assert.NotNull(summaries);
        Assert.NotEmpty(summaries);
        var csCourse = summaries.FirstOrDefault(s => s.CourseTitle.Contains("C#"));
        Assert.NotNull(csCourse);
        Assert.True(csCourse.StudentCount > 0);
        Assert.True(csCourse.AverageGrade > 0);
    }

    [Fact(Skip = "׳©׳׳‘ 4: ׳™׳•׳₪׳¢׳ ׳׳׳—׳¨ ׳׳™׳׳•׳© ׳©׳׳™׳׳×׳× ׳’׳™׳׳™׳•׳ ׳¦׳™׳•׳ ׳™׳ ׳׳¨׳•׳‘׳× ׳˜׳‘׳׳׳•׳×")]
    public async Task GetStudentTranscriptAsync_ReturnsMultiTableJoinedItems()
    {
        var students = await _databaseService.GetStudentsAsync(skip: 0, take: 1);
        Assert.NotEmpty(students);
        int studentId = students[0].Id;

        var transcript = await _databaseService.GetStudentTranscriptAsync(studentId);

        Assert.NotNull(transcript);
        Assert.NotEmpty(transcript);
        Assert.All(transcript, item =>
        {
            Assert.Equal(studentId, item.StudentId);
            Assert.False(string.IsNullOrWhiteSpace(item.CourseTitle));
        });
    }

    [Fact(Skip = "׳©׳׳‘ 4: ׳™׳•׳₪׳¢׳ ׳׳׳—׳¨ ׳׳™׳׳•׳© ׳©׳׳™׳׳×׳× ׳—׳™׳©׳•׳‘ ׳¡׳›׳•׳ ׳ ׳§׳•׳“׳•׳× ׳–׳›׳•׳×")]
    public async Task GetTotalCreditsForStudentAsync_CalculatesSumOfCredits()
    {
        var students = await _databaseService.GetStudentsAsync(skip: 0, take: 1);
        Assert.NotEmpty(students);
        int studentId = students[0].Id;

        int totalCredits = await _databaseService.GetTotalCreditsForStudentAsync(studentId);

        Assert.True(totalCredits > 0);
    }

    [Fact(Skip = "׳©׳׳‘ 4: ׳™׳•׳₪׳¢׳ ׳׳׳—׳¨ ׳׳™׳׳•׳© ׳©׳׳™׳׳×׳× ׳§׳™׳‘׳•׳¥ ׳•׳¡׳₪׳™׳¨׳” ׳©׳ ׳×׳׳׳™׳“׳™׳ ׳׳₪׳™ ׳©׳›׳‘׳”")]
    public async Task GetStudentDistributionByGradeLevelAsync_GroupsAndCountsStudents()
    {
        var distribution = await _databaseService.GetStudentDistributionByGradeLevelAsync();

        Assert.NotNull(distribution);
        Assert.NotEmpty(distribution);
        Assert.All(distribution, d => Assert.True(d.StudentCount > 0));
    }

    [Fact]
    public async Task EnrollStudentAsync_CreatesNewEnrollment()
    {
        var teacher = new Teacher { FullName = "׳׳•׳¨׳” ׳‘׳“׳™׳§׳”", Email = "test@teacher.org", Department = "׳‘׳“׳™׳§׳•׳×" };
        await _databaseService.SaveTeacherAsync(teacher);

        var course = new Course { Title = "׳§׳•׳¨׳¡ ׳‘׳“׳™׳§׳”", Credits = 2, TeacherId = teacher.Id };
        await _databaseService.SaveCourseAsync(course);

        var student = new Student { FirstName = "׳“׳ ׳™", LastName = "׳‘׳“׳™׳§׳”", Email = "dani_test@school.org", GradeLevel = 10 };
        await _databaseService.SaveStudentAsync(student);

        int result = await _databaseService.EnrollStudentAsync(student.Id, course.Id, grade: 99.0);
        Assert.True(result > 0);

        var enrollments = await _databaseService.GetEnrollmentsAsync();
        Assert.Contains(enrollments, e => e.StudentId == student.Id && e.CourseId == course.Id && e.Grade == 99.0);
    }
}
