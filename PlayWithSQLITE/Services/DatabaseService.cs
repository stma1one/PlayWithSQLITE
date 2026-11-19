using PlayWithSQLITE.Models;
using PlayWithSQLITE.Models.DTOs;
using SQLite;

namespace PlayWithSQLITE.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Constants.DatabasePath;
    }

    // ׳‘׳ ׳׳™ ׳ ׳•׳¡׳£ ׳׳”׳–׳¨׳§׳× ׳ ׳×׳™׳‘ ׳׳•׳×׳׳ ג€” ׳׳©׳׳© ׳׳× ׳‘׳“׳™׳§׳•׳× ׳”׳™׳—׳™׳“׳” (xUnit v3)
    public DatabaseService(string dbPath)
    {
        _dbPath = dbPath;
    }

    // ==========================================
    // ׳©׳׳‘ 2: ׳׳×׳—׳•׳ ׳₪׳ ׳™׳׳™ ׳׳•׳˜׳•׳׳˜׳™ (Constants + CreateTableAsync)
    // ==========================================

    private async Task InitDBAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_dbPath, Constants.Flags);

        await _database.CreateTableAsync<Teacher>();
        await _database.CreateTableAsync<Course>();
        await _database.CreateTableAsync<Student>();
        await _database.CreateTableAsync<Enrollment>();
    }

    public async Task ClearAllDataAsync()
    {
        await InitDBAsync();
        // ׳׳—׳™׳§׳” ׳‘׳¡׳“׳¨ ׳”׳׳›׳‘׳“ ׳×׳׳•׳™׳•׳× ׳•׳׳₪׳×׳—׳•׳× ׳–׳¨׳™׳ (׳™׳׳“׳™׳ ׳׳₪׳ ׳™ ׳”׳•׳¨׳™׳)
        await _database!.DeleteAllAsync<Enrollment>();
        await _database.DeleteAllAsync<Course>();
        await _database.DeleteAllAsync<Student>();
        await _database.DeleteAllAsync<Teacher>();
    }

    public async Task SeedSampleDataAsync()
    {
        await InitDBAsync();

        var teacherCount = await _database!.Table<Teacher>().CountAsync();
        if (teacherCount > 0)
            return;

        // 1. ׳”׳•׳¡׳₪׳× ׳׳•׳¨׳™׳
        var teacher1 = new Teacher { FullName = "׳™׳©׳¨׳׳ ׳™׳©׳¨׳׳׳™", Email = "israel@school.org", Department = "׳׳“׳¢׳™ ׳”׳׳—׳©׳‘" };
        var teacher2 = new Teacher { FullName = "׳©׳¨׳” ׳׳•׳™", Email = "sara@school.org", Department = "׳׳×׳׳˜׳™׳§׳”" };
        var teacher3 = new Teacher { FullName = "׳“׳•׳“ ׳›׳”׳", Email = "david@school.org", Department = "׳₪׳™׳–׳™׳§׳”" };

        await _database.InsertAsync(teacher1);
        await _database.InsertAsync(teacher2);
        await _database.InsertAsync(teacher3);

        // 2. ׳”׳•׳¡׳₪׳× ׳§׳•׳¨׳¡׳™׳ ׳¢׳ ׳׳₪׳×׳—׳•׳× ׳–׳¨׳™׳ ׳׳׳•׳¨׳™׳
        var course1 = new Course { Title = "׳×׳›׳ ׳•׳× ׳‘-C# ׳•-MAUI", Credits = 5, Description = "׳₪׳™׳×׳•׳— ׳׳₪׳׳™׳§׳¦׳™׳•׳× ׳׳•׳‘׳™׳™׳ ׳—׳•׳¦׳•׳×-׳₪׳׳˜׳₪׳•׳¨׳׳•׳×", TeacherId = teacher1.Id };
        var course2 = new Course { Title = "׳׳‘׳ ׳™ ׳ ׳×׳•׳ ׳™׳ ׳•׳׳׳’׳•׳¨׳™׳×׳׳™׳", Credits = 4, Description = "׳™׳¡׳•׳“׳•׳× ׳×׳™׳׳•׳¨׳˜׳™׳™׳ ׳•׳׳¢׳©׳™׳™׳", TeacherId = teacher1.Id };
        var course3 = new Course { Title = "׳—׳“׳•\"׳ ׳׳×׳׳׳™׳“׳™ ׳׳“׳¢׳™ ׳”׳׳—׳©׳‘", Credits = 4, Description = "׳—׳©׳‘׳•׳ ׳׳™׳ ׳₪׳™׳ ׳™׳˜׳¡׳™׳׳׳™", TeacherId = teacher2.Id };
        var course4 = new Course { Title = "׳׳›׳ ׳™׳§׳” ׳§׳׳׳¡׳™׳× ׳•׳—׳©׳׳", Credits = 3, Description = "׳׳¢׳‘׳“׳•׳× ׳•׳₪׳™׳–׳™׳§׳” ׳׳¢׳©׳™׳×", TeacherId = teacher3.Id };

        await _database.InsertAsync(course1);
        await _database.InsertAsync(course2);
        await _database.InsertAsync(course3);
        await _database.InsertAsync(course4);

        // 3. ׳”׳•׳¡׳₪׳× ׳×׳׳׳™׳“׳™׳
        var students = new List<Student>
        {
            new() { FirstName = "׳ ׳•׳¢׳", LastName = "׳׳‘׳¨׳”׳", Email = "noam@students.org", GradeLevel = 11, BirthDate = new DateTime(2009, 3, 15) },
            new() { FirstName = "׳׳׳™׳”", LastName = "׳©׳׳•׳", Email = "maya@students.org", GradeLevel = 11, BirthDate = new DateTime(2009, 7, 22) },
            new() { FirstName = "׳™׳•׳ ׳×׳", LastName = "׳׳–׳¨׳—׳™", Email = "yonatan@students.org", GradeLevel = 12, BirthDate = new DateTime(2008, 1, 10) },
            new() { FirstName = "׳×׳׳¨", LastName = "׳’׳‘׳׳™", Email = "tamar@students.org", GradeLevel = 12, BirthDate = new DateTime(2008, 11, 5) },
            new() { FirstName = "׳׳•׳¨׳™", LastName = "׳׳׳•׳", Email = "ori@students.org", GradeLevel = 10, BirthDate = new DateTime(2010, 5, 30) },
            new() { FirstName = "׳¨׳•׳ ׳™", LastName = "׳₪׳¨׳¥", Email = "roni@students.org", GradeLevel = 10, BirthDate = new DateTime(2010, 9, 18) }
        };

        foreach (var s in students)
        {
            await _database.InsertAsync(s);
        }

        // 4. ׳¨׳™׳©׳•׳ ׳×׳׳׳™׳“׳™׳ ׳׳§׳•׳¨׳¡׳™׳ (׳˜׳‘׳׳× ׳’׳™׳©׳•׳¨ Enrollment)
        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = course1.Id, Grade = 95, IsPaid = true },
            new() { StudentId = students[0].Id, CourseId = course2.Id, Grade = 88, IsPaid = true },
            new() { StudentId = students[1].Id, CourseId = course1.Id, Grade = 92, IsPaid = true },
            new() { StudentId = students[1].Id, CourseId = course3.Id, Grade = 85, IsPaid = true },
            new() { StudentId = students[2].Id, CourseId = course1.Id, Grade = 78, IsPaid = true },
            new() { StudentId = students[2].Id, CourseId = course4.Id, Grade = 91, IsPaid = false },
            new() { StudentId = students[3].Id, CourseId = course2.Id, Grade = 97, IsPaid = true },
            new() { StudentId = students[3].Id, CourseId = course3.Id, Grade = 94, IsPaid = true },
            new() { StudentId = students[4].Id, CourseId = course4.Id, Grade = 82, IsPaid = true },
            new() { StudentId = students[5].Id, CourseId = course1.Id, Grade = 89, IsPaid = true }
        };

        foreach (var e in enrollments)
        {
            await _database.InsertAsync(e);
        }
    }

    // ==========================================
    // ׳©׳׳‘ 2: CRUD ׳‘׳¡׳™׳¡׳™ ג€” ׳׳•׳¨׳™׳ (Teachers)
    // ==========================================

    public async Task<List<Teacher>> GetTeachersAsync()
    {
        await InitDBAsync();
        return await _database!.Table<Teacher>().OrderBy(t => t.FullName).ToListAsync();
    }

    public async Task<Teacher?> GetTeacherByIdAsync(int id, bool includeCourses = false)
    {
        await InitDBAsync();
        // ׳‘׳©׳׳‘ 3 ׳ ׳©׳“׳¨׳’ ׳׳˜׳¢׳™׳ ׳× ׳§׳•׳¨׳¡׳™׳ ׳›׳׳©׳¨ includeCourses == true
        return await _database!.Table<Teacher>().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> SaveTeacherAsync(Teacher teacher)
    {
        await InitDBAsync();
        if (teacher.Id != 0)
            return await _database!.UpdateAsync(teacher);
        return await _database!.InsertAsync(teacher);
    }

    public async Task<int> DeleteTeacherAsync(Teacher teacher)
    {
        await InitDBAsync();
        return await _database!.DeleteAsync(teacher);
    }

    // ==========================================
    // ׳©׳׳‘ 2: CRUD ׳‘׳¡׳™׳¡׳™ ג€” ׳§׳•׳¨׳¡׳™׳ (Courses)
    // ==========================================

    public async Task<List<Course>> GetCoursesAsync(bool includeRelations = false)
    {
        await InitDBAsync();
        // ׳‘׳©׳׳‘ 3 ׳ ׳©׳“׳¨׳’ ׳׳˜׳¢׳™׳ ׳× ׳§׳©׳¨׳™׳ ׳›׳׳©׳¨ includeRelations == true
        return await _database!.Table<Course>().OrderBy(c => c.Title).ToListAsync();
    }

    public async Task<Course?> GetCourseByIdAsync(int id, bool includeRelations = true)
    {
        await InitDBAsync();
        return await _database!.Table<Course>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> SaveCourseAsync(Course course)
    {
        await InitDBAsync();
        if (course.Id != 0)
            return await _database!.UpdateAsync(course);
        return await _database!.InsertAsync(course);
    }

    public async Task<int> DeleteCourseAsync(Course course)
    {
        await InitDBAsync();
        return await _database!.DeleteAsync(course);
    }

    // ==========================================
    // ׳©׳׳‘ 2: CRUD ׳‘׳¡׳™׳¡׳™ ג€” ׳×׳׳׳™׳“׳™׳ (Students)
    // ==========================================

    public async Task<List<Student>> GetStudentsAsync(int skip = 0, int take = 20)
    {
        await InitDBAsync();
        // ׳‘׳©׳׳‘ 4 ׳ ׳•׳¡׳™׳£ Skip(skip) ׳•-Take(take) ׳׳“׳₪׳“׳•׳£
        return await _database!.Table<Student>()
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();
    }

    public async Task<int> GetStudentsCountAsync()
    {
        await InitDBAsync();
        return await _database!.Table<Student>().CountAsync();
    }

    public async Task<Student?> GetStudentByIdAsync(int id, bool includeCourses = false)
    {
        await InitDBAsync();
        return await _database!.Table<Student>().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int> SaveStudentAsync(Student student)
    {
        await InitDBAsync();
        if (student.Id != 0)
            return await _database!.UpdateAsync(student);
        return await _database!.InsertAsync(student);
    }

    public async Task<int> DeleteStudentAsync(Student student)
    {
        await InitDBAsync();
        return await _database!.DeleteAsync(student);
    }

    // ==========================================
    // ׳©׳׳‘ 3: ׳§׳©׳¨׳™ ׳’׳•׳׳׳™׳ ׳•׳¨׳™׳©׳•׳׳™׳ (׳™׳•׳©׳׳ ׳‘׳©׳׳‘ 3)
    // ==========================================

    public async Task<List<Enrollment>> GetEnrollmentsAsync() =>
        await Task.FromResult(new List<Enrollment>());

    public async Task<int> EnrollStudentAsync(int studentId, int courseId, double? grade = null) =>
        await Task.FromResult(0);

    public async Task<int> UpdateEnrollmentGradeAsync(int enrollmentId, double grade) =>
        await Task.FromResult(0);

    public async Task<int> RemoveEnrollmentAsync(int studentId, int courseId) =>
        await Task.FromResult(0);

    // ==========================================
    // ׳©׳׳‘ 4: ׳©׳׳™׳׳×׳•׳× ׳׳•׳¨׳›׳‘׳•׳× (׳™׳•׳©׳׳ ׳‘׳©׳׳‘ 4)
    // ==========================================

    public async Task<List<CourseGradeSummary>> GetCourseGradeSummariesAsync() =>
        await Task.FromResult(new List<CourseGradeSummary>());

    public async Task<List<StudentTranscriptItem>> GetStudentTranscriptAsync(int studentId) =>
        await Task.FromResult(new List<StudentTranscriptItem>());

    public async Task<List<GradeLevelCount>> GetStudentDistributionByGradeLevelAsync() =>
        await Task.FromResult(new List<GradeLevelCount>());

    public async Task<int> GetTotalCreditsForStudentAsync(int studentId) =>
        await Task.FromResult(0);
}
