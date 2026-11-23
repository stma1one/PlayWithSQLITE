using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PlayWithSQLITE.Models;

[Table("Students")]
public class Student
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [NotNull, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [NotNull, Unique, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public int GradeLevel { get; set; } = 10;
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-16);

    // קשר M:N מול קורסים דרך טבלת הגישור Enrollment (ללא CascadeDelete כדי לא למחוק קורסים!)
    [ManyToMany(typeof(Enrollment), CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
    public List<Course> Courses { get; set; } = new();

    // שדות מחושבים לתצוגה בממשק בלבד
    [Ignore]
    public string FullName => $"{FirstName} {LastName}";

    [Ignore]
    public string GradeLevelHebrew => GradeLevel switch
    {
        10 => "שכבה י'",
        11 => "שכבה י\"א",
        12 => "שכבה י\"ב",
        _ => $"שכבה {GradeLevel}"
    };
}
