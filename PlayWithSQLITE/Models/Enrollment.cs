using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PlayWithSQLITE.Models;

[Table("Enrollments")]
public class Enrollment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // מפתח זר לתלמיד (צד ה-N בקשר מול Students)
    [ForeignKey(typeof(Student))]
    public int StudentId { get; set; }

    // מפתח זר לקורס (צד ה-N בקשר מול Courses)
    [ForeignKey(typeof(Course))]
    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    // ציון בקורס (0-100) — nullable כל עוד טרם הוזן ציון סופי
    public double? Grade { get; set; }

    // האם שולמו דמי הרישום
    public bool IsPaid { get; set; } = true;

    // מאפייני ניווט בצד ה-N לשתי ישויות ההורה
    [ManyToOne]
    public Student? Student { get; set; }

    [ManyToOne]
    public Course? Course { get; set; }
}
