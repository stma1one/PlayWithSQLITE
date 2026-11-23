# 📦 שלב 1: התקנת חבילות NuGet, תכנון ERD, קשרי גומלין, `CascadeOperation` ודקורציות במודלים

בשלב זה נניח את **תשתית הנתונים** של המערכת:
1. נתקין בצורה ויזואלית את חבילות ה-NuGet היציבות (`Stable`) של SQLite לפי סביבת הפיתוח שלכם (Visual Studio או Rider).
2. נלמד לקרוא **תרשים קשרי ישויות (ERD)** ל-4 הטבלאות של מערכת בית הספר.
3. נבין לעומק **מהו קשר 1:N ומהו קשר M:N**, ו**איזה קישוט מגדירים בכל צד של הקשר**.
4. נלמד מהי פעולת **`CascadeOperation`** ומתי בוחרים כל ערך שלה.
5. נלמד מתוך **דוגמה מלאה בעולם תוכן אחר (ספרייה דיגיטלית)** איך נראים קובצי מודל גמורים — ואז ניישם בעצמנו על 4 המודלים של בית הספר!

---

## 1. התקנת חבילות ה-NuGet המאושרות (ממשק ויזואלי — Stable בלבד!)

בפרויקט .NET MAUI נשתמש בחבילות המאושרות הבאות (בגרסאות יציבות — `Stable` בלבד):
1. **`sqlite-net-pcl`** (גרסה יציבה — Stable, למשל `1.11.285`): חבילת הבסיס האסינכרונית המספקת את `SQLiteAsyncConnection`, את דגלי הפתיחה `SQLiteOpenFlags`, ואת דקורציות הטבלאות (`[Table]`, `[PrimaryKey]` ועוד).
2. **`SQLiteNetExtensions.Async`** + **`SQLiteNetExtensions`** (גרסה יציבה — Stable `2.1.0`): הרחבה מעל `sqlite-net-pcl` המאפשרת להגדיר ולטעון קשרי גומלין (`1:1`, `1:N`, `M:N`) באופן אסינכרוני.
3. **`Newtonsoft.Json`** (גרסה יציבה אחרונה — **Latest stable `13.0.4`**): **עדכון אבטחה חובה בכל הפרויקטים!** חבילת ה-SQLite Extensions מושכת כתלות פנימית גרסה ישנה ולא מעודכנת של `Newtonsoft.Json` (`9.0.1`) שגורמת לאזהרת אבטחה (`NU1903`). עדכון מפורש של `Newtonsoft.Json` לגרסה היציבה האחרונה בשני הפרויקטים (`PlayWithSQLITE` ו-`PlayWithSQLITE.Tests`) פותר את האזהרה לחלוטין!

> [!IMPORTANT]
> **כלל ברזל — גרסאות יציבות (`Stable`) בלבד!**
> בעת החיפוש במנהל החבילות, **ודאו שתיבת הסימון "Include prerelease" / "Prerelease" אינה מסומנת**. התקנת גרסאות ניסיוניות (Beta / Preview) עלולה לגרום לשגיאות ריצה או אי-תאימות.

### 🖥️ אפשרות א': Visual Studio (Windows — VS 2026 / VS 2022)
*(עפ"י התיעוד הרשמי של Microsoft: `Install and use a package in Visual Studio`)*
1. **פתיחת מנהל החבילות עבור כל הפתרון (Solution):**  
   - בחלון **Solution Explorer**, בצעו קליק ימני על שורת ה-**Solution (`PlayWithSQLITE`)** ובחרו **`Manage NuGet Packages for Solution...`**  
   *(חלופה מהתפריט העליון: `Tools` ➡️ `NuGet Package Manager` ➡️ `Manage NuGet Packages for Solution...`)*.
2. **הגדרת מקור וחיפוש:**
   - לחצו על הלשונית **`Browse`** (עיון) בחלק העליון.
   - ודאו שבתפריט **`Package source`** (בפינה הימנית העליונה) נבחר **`nuget.org`**.
   - ודאו שתיבת **`Include prerelease`** לצד שורת החיפוש **אינה מסומנת (Unchecked)**.
3. **התקנת החבילה הראשונה (`sqlite-net-pcl`):**
   - הקלידו בשורת החיפוש: `sqlite-net-pcl`
   - בחרו בחבילה הרשמית **`sqlite-net-pcl`** (מאת `Frank A. Krueger / Eric Sink`).  
     *(שימו לב: לא לבחור `SQLite.Net-PCL` הישנה ולא `Microsoft.Data.Sqlite`)*.
   - בחלונית הפרטים בצד ימין: סמנו ב-✅ את **`PlayWithSQLITE`**, ודאו שבשדה `Version` נבחרה **Latest stable**, ולחצו **`Install`**.
   - בחלון **Preview Changes** לחצו **`OK`**, ואשרו את חלון הרישיון (**I Accept**) אם יופיע.
4. **התקנת חבילות הקשרים (`SQLiteNetExtensions.Async` ו-`SQLiteNetExtensions`):**
   - הקלידו `SQLiteNetExtensions.Async`, בחרו בגרסה היציבה (`2.1.0`), סמנו ב-✅ את **`PlayWithSQLITE`** ו-**`PlayWithSQLITE.Tests`**, ולחצו **`Install`** ➡️ **`OK`**.
   - התקינו באותו אופן גם את **`SQLiteNetExtensions`** בגרסה היציבה (`2.1.0`) בשני הפרויקטים (כדי להחליף את התלות הישנה `2.0.0` ולהעלים אזהרות תאימות `NU1701`).
5. **עדכון `Newtonsoft.Json` לגרסה היציבה האחרונה בכל הפרויקטים (חובה!):**
   - הקלידו בשורת החיפוש: `Newtonsoft.Json`
   - בחרו בחבילה הרשמית **`Newtonsoft.Json`** (מאת `James Newton-King`), ודאו שבשדה `Version` נבחרה **Latest stable (`13.0.4`)**, סמנו ב-✅ את **שני הפרויקטים (`PlayWithSQLITE` ו-`PlayWithSQLITE.Tests`)**, ולחצו **`Install` / `Update`** ➡️ **`OK`**.

### 🍎 אפשרות ב': JetBrains Rider (macOS / Mac)
*(עפ"י התיעוד הרשמי של JetBrains: `Consume NuGet packages in Rider`)*
1. **פתיחת חלון ה-NuGet Tool Window:**  
   - פתחו את החלון הייעודי דרך התפריט העליון: **`View`** ➡️ **`Tool Windows`** ➡️ **`NuGet`**  
   *(חלופה מהירה: קליק ימני על ה-Solution ב-Solution Explorer ➡️ `Manage NuGet Packages`)*.
2. **בדיקת Prerelease וחיפוש:**
   - בחלון ה-**NuGet** שנפתח (בלשונית **Packages**), ודאו שתיבת/מתג **`Prerelease`** ליד שורת החיפוש **מכובה (אינו מסומן)**.
   - הקלידו בשורת החיפוש (צד שמאל): `sqlite-net-pcl`.
3. **בחירת הפרויקט והתקנה:**
   - לחצו על התוצאה **`sqlite-net-pcl`** ברשימה השמאלית.
   - בחלונית הפרטים בצד ימין תראו את רשימת הפרויקטים ב-Solution ואת תיבת בחירת הגרסה (`Version`).
   - ודאו שנבחרה הגרסה היציבה (**Latest stable**), ולחצו על כפתור הפלוס הירוק (**`+` / `Install`**) בשורה של פרויקט **`PlayWithSQLITE`**.
4. **התקנת חבילות הקשרים (`SQLiteNetExtensions.Async` ו-`SQLiteNetExtensions`):**
   - חפשו `SQLiteNetExtensions.Async` (גרסה `2.1.0`) ולאחריה `SQLiteNetExtensions` (גרסה `2.1.0`), ולחצו על **`+` / `Install`** עבור **`PlayWithSQLITE`** ו-**`PlayWithSQLITE.Tests`**.
5. **עדכון `Newtonsoft.Json` לגרסה היציבה האחרונה בכל הפרויקטים (חובה!):**
   - חפשו `Newtonsoft.Json`, בחרו בגרסה היציבה האחרונה (**Latest stable `13.0.4`**), ולחצו **`+` / `Install`** (או `Upgrade`) עבור **שני הפרויקטים (`PlayWithSQLITE` ו-`PlayWithSQLITE.Tests`)** כדי לעדכן את הגרסה הישנה (`9.0.1`) שמגיעה עם חבילת ההרחבה.

> [!TIP]
> **איך מוודאים שההתקנה הצליחה?**
> ב-Solution Explorer, פתחו את `PlayWithSQLITE` ➡️ `Dependencies` ➡️ `Packages` וודאו שכל 4 החבילות (`sqlite-net-pcl`, `SQLiteNetExtensions`, `SQLiteNetExtensions.Async`, `Newtonsoft.Json 13.0.4`) מופיעות ללא אזהרות אבטחה (`NU1903`), וש-**Build Solution** עובר עם **0 Warnings ו-0 Errors**.

---

## 2. תרשים קשרי ישויות (ERD — Entity-Relationship Diagram) של המערכת שלנו

לפני שניגשים לקוד, נלמד את תרשים ה-**ERD** של מערכת בית הספר שלנו — 4 טבלאות, מפתחות ראשיים (`PK`), מפתחות זרים (`FK`), וקשרי `1:N` ו-`M:N`:

![תרשים קשרי ישויות ERD — PlayWithSQLITE](../Images/Database-ERD.png)

---

## 3. איך מגדירים קשרים בין טבלאות — ומה מוגדר באיזה צד?

### א. קשר אחד-לרבים (`1:N`) — מורה וקורסים (`Teacher` ⟷ `Course`)
| צד הקשר | המחלקה | מה מוגדר במחלקה זו? | למה דווקא בצד הזה? |
| :--- | :--- | :--- | :--- |
| **צד ה-1 (הורה)** | `Teacher` | רשימת הילדים `List<Course> Courses` עם **`[OneToMany]`** | בטבלת `Teachers` אין עמודת `CourseId` — אי אפשר לשמור רשימת מזהים בתא אחד בטבלה! |
| **צד ה-N (ילד)** | `Course` | 1. מפתח זר: `int TeacherId` עם **`[ForeignKey(typeof(Teacher))]`**<br/>2. מאפיין ניווט: `Teacher? Teacher` עם **`[ManyToOne]`** | **כלל ברזל:** המפתח הזר (`FK`) יושב **תמיד** בצד ה-**N**! כל שורת קורס מצביעה למורה אחד. |

### ב. קשר רבים-לרבים (`M:N`) — תלמידים וקורסים דרך טבלת הגישור (`Enrollment`)
| תפקיד בקשר | המחלקה | מה מוגדר במחלקה זו? | תפקידו במערכת |
| :--- | :--- | :--- | :--- |
| **טבלת הגישור (צד N לשניהם)** | `Enrollment` | שני מפתחות זרים: `StudentId` (`[ForeignKey(typeof(Student))]`) ו-`CourseId` (`[ForeignKey(typeof(Course))]`) + שני מאפייני ניווט **`[ManyToOne]`** (`Student?`, `Course?`) | שוברת קשר `M:N` לשני קשרי `1:N`, ושומרת מידע ייחודי לרישום (`Grade`, `EnrollmentDate`, `IsPaid`). |
| **שני הצדדים של ה-M:N** | `Student` ו-`Course` | `List<Course> Courses` ו-`List<Student> Students` עם **`[ManyToMany(typeof(Enrollment))]`** | מאפשרים ניווט ישיר מהתלמיד לקורסיו ומהקורס לתלמידיו דרך טבלת הגישור. |

---

## 4. מהו `CascadeOperation` ומתי בוחרים כל ערך?

הפרמטר `CascadeOperations` בקישוטי `SQLiteNetExtensions` קובע האם פעולה על ישות ההורה "תתגלגל" אוטומטית גם לישויות המקושרות אליה:

| ערך (`CascadeOperation`) | מה הוא עושה בפועל? | מתי משתמשים בו? |
| :--- | :--- | :--- |
| **`None`** *(ברירת מחדל)* | שום פעולה לא מתגלגלת אוטומטית. | כשרוצים שליטה ידנית מלאה. |
| **`CascadeRead`** | בקריאה עם `GetWithChildrenAsync` הילדים נטענים אוטומטית מהמסד. | כשרוצים לקבל את הילדים יחד עם ההורה. |
| **`CascadeInsert`** | בשמירה עם `InsertWithChildrenAsync` גם ילדים חדשים נשמרים אוטומטית. | כשיוצרים הורה וילדים יחד בזיכרון. |
| **`CascadeDelete`** | במחיקה רקורסיבית, גם הילדים נמחקים אוטומטית (מניעת רשומות יתומות). | **רק** כשהילדים לא יכולים להתקיים ללא ההורה! |
| **`All`** | שילוב של `CascadeRead \| CascadeInsert \| CascadeDelete`. | מתאים לקשר בעלות `1:N` (כמו `Teacher.Courses`). |

> [!WARNING]
> **זהירות בקשר `M:N`!**
> בקשר רבים-לרבים (`Student` ⟷ `Course`), **לא** נרצה להשתמש ב-`CascadeDelete` אוטומטי, כי מחיקת קורס עלולה למחוק את **התלמידים עצמם** במקום רק את הרישומים שלהם!  
> לכן בקשר `M:N` נבחר **`CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert`** (ולא `All`), ובשלב 3 נמחק באופן מבוקר רק את שורות ה-`Enrollment`.

---

## 5. מילון הדקורציות (`Attributes`)

| דקורציה | מרחב שמות (`using`) | תפקיד ומשמעות |
| :--- | :--- | :--- |
| `[Table("Name")]` | `SQLite` | קובעת את שם הטבלה במסד הנתונים. |
| `[PrimaryKey, AutoIncrement]` | `SQLite` | מפתח ראשי ייחודי עם מספור אוטומטי (`1, 2, 3...`). |
| `[NotNull]` | `SQLite` | אוסרת ערך `NULL` (שדה חובה). |
| `[Unique]` | `SQLite` | אוסרת כפילות ערכים בעמודה (למשל `Email` או `CardNumber`). |
| `[MaxLength(N)]` | `SQLite` | מגבילה את אורך הטקסט המרבי. |
| `[Ignore]` | `SQLite` | מורה ל-SQLite **לא** ליצור עמודה עבור מאפיין מחושב (ל-UI בלבד). |
| `[ForeignKey(typeof(T))]` | `SQLiteNetExtensions.Attributes` | מסמנת עמודה כמפתח זר המצביע לטבלה של `T`. |
| `[OneToMany(CascadeOperations = ...)]` | `SQLiteNetExtensions.Attributes` | מונחת על `List<Child>` בצד ה-**1**. |
| `[ManyToOne]` | `SQLiteNetExtensions.Attributes` | מונחת על ישות ההורה בצד ה-**N**. |
| `[ManyToMany(typeof(Junction), CascadeOperations = ...)]` | `SQLiteNetExtensions.Attributes` | מונחת על הרשימות בשני הצדדים של קשר `M:N`. |

---

## 6. דוגמה מלאה להשראה (עולם תוכן אחר: ספרייה דיגיטלית) + משימת התלמיד

כדי שתראו בדיוק איך כל הדקורציות עובדות יחד — **בלי שנפתור במקומכם את מערכת בית הספר** — נתבונן במערכת **ספרייה דיגיטלית** בעלת מבנה קשרים זהה:
- **קשר `1:N`:** סופר (`Author`) כותב ספרים רבים (`Book`).
- **קשר `M:N`:** קוראים (`Reader`) שואלים ספרים רבים (`Book`) דרך טבלת הגישור **`BookLoan`** (השאלה).

![דוגמת השראה ERD — מערכת ספרייה דיגיטלית](../Images/Library-Mini-ERD.png)

### טבלת מיפוי מלאה: כל מחלקה ותכונה בדוגמת הספרייה, ה-Decoration שלה וההסבר

לפני שנראה את קוד ה-C#, עברו על הטבלה ושימו לב כיצד כל החלטה מתרשים ה-ERD מתורגמת לדקורציה מדויקת:

| מחלקה / תכונה | הדקורציה (`Decoration`) | הסבר — למה בחרנו דווקא בזו? |
| :--- | :--- | :--- |
| **`class Author`** | `[Table("Authors")]` | קובעת את שם הטבלה במסד הנתונים (`Authors`). |
| `Author.Id` | `[PrimaryKey, AutoIncrement]` | מפתח ראשי חד-ערכי שממוספר אוטומטית (`1, 2, 3...`). |
| `Author.Name` | `[NotNull, MaxLength(80)]` | שדה חובה (אינו יכול להיות `NULL`) באורך של עד 80 תווים. |
| `Author.Books` | `[OneToMany(CascadeOperations = CascadeOperation.All)]` | צד ה-**1** בקשר `1:N` (סופר אחד כותב ספרים רבים); פעולות קריאה/שמירה/מחיקה מתגלגלות לספריו. |
| `Author.Summary` | `[Ignore]` | מאפיין מחושב לתצוגה בממשק בלבד — **לא** תיווצר עבורו עמודה בטבלה. |
| **`class Book`** | `[Table("Books")]` | קובעת את שם הטבלה במסד (`Books`). |
| `Book.Id` | `[PrimaryKey, AutoIncrement]` | מפתח ראשי ומספור אוטומטי לכל ספר. |
| `Book.Title` | `[NotNull, MaxLength(120)]` | כותרת הספר היא שדה חובה עד 120 תווים. |
| `Book.AuthorId` | `[ForeignKey(typeof(Author))]` | צד ה-**N** בקשר `1:N` — המפתח הזר יושב **תמיד** בצד ה-N ומצביע אל `Author`. |
| `Book.Author` | `[ManyToOne]` | מאפיין ניווט לסופר האחד שכתב את הספר (נטען אוטומטית ב-`GetWithChildrenAsync`). |
| `Book.Readers` | `[ManyToMany(typeof(BookLoan), CascadeOperations = CascadeOperation.CascadeRead \| CascadeOperation.CascadeInsert)]` | קשר `M:N` מול קוראים דרך טבלת הגישור `BookLoan` — **בלי `CascadeDelete`** כדי שמחיקת ספר לא תמחק קוראים! |
| `Book.AuthorName` | `[Ignore]` | שדה נוחות ל-XAML בלבד, אינו עמודה בטבלה. |
| **`class Reader`** | `[Table("Readers")]` | קובעת את שם הטבלה (`Readers`). |
| `Reader.Id` | `[PrimaryKey, AutoIncrement]` | מפתח ראשי אוטומטי לקורא. |
| `Reader.FullName` | `[NotNull, MaxLength(80)]` | שם חובה עד 80 תווים. |
| `Reader.CardNumber` | `[NotNull, Unique, MaxLength(20)]` | מספר כרטיס ספרייה חובה ו**ייחודי** — המסד מונע כפילות! |
| `Reader.Books` | `[ManyToMany(typeof(BookLoan), CascadeOperations = CascadeOperation.CascadeRead \| CascadeOperation.CascadeInsert)]` | הצד השני של קשר ה-`M:N` דרך אותה טבלת גישור `BookLoan`. |
| **`class BookLoan`** | `[Table("BookLoans")]` | טבלת הגישור (Junction Table) השוברת את קשר ה-`M:N`. |
| `BookLoan.Id` | `[PrimaryKey, AutoIncrement]` | מפתח ראשי אוטומטי לכל רשומת השאלה. |
| `BookLoan.BookId` | `[ForeignKey(typeof(Book))]` | מפתח זר ראשון המצביע אל טבלת `Books`. |
| `BookLoan.ReaderId` | `[ForeignKey(typeof(Reader))]` | מפתח זר שני המצביע אל טבלת `Readers`. |
| `BookLoan.Book` / `Reader` | `[ManyToOne]` | מאפייני ניווט בצד ה-N לשתי ישויות ההורה של ההשאלה. |

### המימוש המלא של 4 המחלקות בדוגמת הספרייה:

```csharp
using SQLite;
using SQLiteNetExtensions.Attributes;

// 1. צד ה-1 (הורה): סופר אחד כותב ספרים רבים
[Table("Authors")]
public class Author
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Book> Books { get; set; } = new();

    [Ignore] // מאפיין תצוגה מחושב — לא נשמר בעמודה במסד!
    public string Summary => $"{Name} ({Books.Count} ספרים)";
}

// 2. צד ה-N לסופר + צד M:N לקוראים
[Table("Books")]
public class Book
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [ForeignKey(typeof(Author))] // המפתח הזר תמיד בצד ה-N!
    public int AuthorId { get; set; }

    [ManyToOne] // ניווט אל הסופר האחד
    public Author? Author { get; set; }

    [ManyToMany(typeof(BookLoan), CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
    public List<Reader> Readers { get; set; } = new();

    [Ignore]
    public string AuthorName => Author?.Name ?? "לא ידוע";
}

// 3. צד שני של ה-M:N: קורא עם מספר כרטיס ייחודי (Unique)
[Table("Readers")]
public class Reader
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(80)]
    public string FullName { get; set; } = string.Empty;

    [NotNull, Unique, MaxLength(20)]
    public string CardNumber { get; set; } = string.Empty;

    [ManyToMany(typeof(BookLoan), CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
    public List<Book> Books { get; set; } = new();
}

// 4. טבלת הגישור (Junction Table): שוברת את ה-M:N לשני קשרי 1:N
[Table("BookLoans")]
public class BookLoan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [ForeignKey(typeof(Book))]
    public int BookId { get; set; }

    [ForeignKey(typeof(Reader))]
    public int ReaderId { get; set; }

    public DateTime LoanDate { get; set; } = DateTime.Now;

    [ManyToOne]
    public Book? Book { get; set; }

    [ManyToOne]
    public Reader? Reader { get; set; }
}
```

---

### ✍️ עכשיו תורכם! משימת שלב 1 בפרויקט בית הספר (`PlayWithSQLITE/Models/`)

לפי הדוגמה של הספרייה ולפי תרשים ה-ERD של בית הספר בסעיף 2, פתחו כל אחד מ-4 קובצי המודל וענו על ההערות המנחות שבו:

| קובץ מודל | מהו התפקיד שלו ב-ERD? | מה עליכם להוסיף? |
| :--- | :--- | :--- |
| **`Teacher.cs`** | מקביל ל-`Author` (צד ה-1) | `using`s, שם טבלה `"Teachers"`, מפתח ראשי אוטומטי, אילוצי `NotNull`/`MaxLength`, קשר `[OneToMany]` ל-`Courses` עם `CascadeOperation.All`, ו-`[Ignore]` על `DisplayInfo`. |
| **`Course.cs`** | מקביל ל-`Book` (צד N למורה + M:N לתלמידים) | `using`s, שם טבלה `"Courses"`, מפתח ראשי, אילוצי `Title`/`Description`, `[ForeignKey]` על `TeacherId`, `[ManyToOne]` על `Teacher`, `[ManyToMany]` על `Students` דרך `Enrollment` (ללא `CascadeDelete`!), ו-`[Ignore]` על `TeacherName`. |
| **`Student.cs`** | מקביל ל-`Reader` (צד M:N לקורסים) | `using`s, שם טבלה `"Students"`, מפתח ראשי, אילוצי שם פרטי ומשפחה, `[NotNull, Unique, MaxLength(100)]` על `Email`, `[ManyToMany]` על `Courses`, ו-`[Ignore]` על **שני** השדות המחושבים (`FullName` ו-`GradeLevelHebrew`). |
| **`Enrollment.cs`** | מקביל ל-`BookLoan` (טבלת גישור) | `using`s, שם טבלה `"Enrollments"`, מפתח ראשי, שני `[ForeignKey]` (`StudentId`, `CourseId`), ושני `[ManyToOne]` (`Student`, `Course`). |

### ✅ בדיקה עצמית בסיום שלב 1:
- הריצו **Build Solution** ב-Visual Studio או ב-Rider וודאו שיש **0 שגיאות קומפילציה**.
- להשוואת הפתרון שלכם מול הענף המוכן: `git checkout step-01-packages-and-models`.

עברו לשלב הבא: [02-Database-Connection-And-Basic-CRUD.pdf](./02-Database-Connection-And-Basic-CRUD.pdf) *(ומומלץ לקרוא גם את [00b-How-To-Run-Tests.pdf](./00b-How-To-Run-Tests.pdf) לפני תחילת שלב 2!)*.
