using SQLite;

namespace PlayWithSQLITE;

/// <summary>
/// מחלקה סטטית לריכוז קבועי ההגדרה של מסד הנתונים (מיקום הקובץ והרשאות הפתיחה).
/// </summary>
public static class Constants
{
    public const string DatabaseFilename = "SchoolLms.db3";

    public const SQLiteOpenFlags Flags =
        // פתיחת המסד לקריאה ולכתיבה
        SQLiteOpenFlags.ReadWrite |
        // יצירת קובץ המסד אוטומטית אם אינו קיים עדיין
        SQLiteOpenFlags.Create |
        // הפעלת מטמון משותף לגישה בטוחה ממספר תהליכונים
        SQLiteOpenFlags.SharedCache;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}
