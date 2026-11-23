using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PlayWithSQLITE.Models;

[Table("Courses")]
public class Course
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public int Credits { get; set; } = 3;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    // קשר 1:N (צד ה-N / ילד) — המפתח הזר יושב תמיד בצד ה-N!
    [ForeignKey(typeof(Teacher))]
    public int TeacherId { get; set; }

    // מאפיין ניווט להורה (רבים-ליחיד)
    [ManyToOne]
    public Teacher? Teacher { get; set; }

    // קשר M:N מול תלמידים דרך טבלת הגישור Enrollment (ללא CascadeDelete כדי לא למחוק תלמידים!)
    [ManyToMany(typeof(Enrollment), CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
    public List<Student> Students { get; set; } = new();

    // שדה מחושב לתצוגה ב-XAML בלבד
    [Ignore]
    public string TeacherName => Teacher?.FullName ?? $"מורה מזהה #{TeacherId}";
}
