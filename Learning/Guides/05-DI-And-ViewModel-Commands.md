# 🧩 שלב 5: רישום ב-DI Container, יצירת פקודות (`ICommand`) ב-ViewModels ויישום דפדוף (Pagination)

עד שלב 4 בנינו ואמתנו את **שכבת הנתונים** במלואה (`Constants`, `Models`, `IDatabaseService`, `DatabaseService`).  
בשלב 5 נחבר את שכבת מסד הנתונים אל ארבעת מסכי ה-UI שלנו בשני חלקים:
1. רישום השירות ב-**DI Container** (`MauiProgram.cs`).
2. בניית פקודות **`ICommand`** ב-ViewModels (בגישת Vanilla C# MVVM) להזרמת נתונים מהמסד אל המסך — כולל **דוגמה מלאה מקצה לקצה** של מסך המורים (`TeachersViewModel`) ו**מדריך יישומי שלב-אחר-שלב לדפדוף (`Pagination`)** במסך התלמידים!

> [!NOTE]
> **שימו לב להפרדת שכבות נקייה:**  
> אף ViewModel אינו קורא ל-`InitDBAsync()`! אתחול החיבור ויצירת הטבלאות מתבצעים אוטומטית בתוך `DatabaseService` (`private InitDBAsync()`). ה-ViewModel רק קורא למתודות העסקיות של `IDatabaseService` (`GetTeachersAsync`, `SaveStudentAsync` וכו').

---

## חלק 1: רישום השירות ב-DI Container (`MauiProgram.cs`)

### למה **אסור** לכתוב `new DatabaseService()` בתוך כל ViewModel?
1. פתיחת מספר מופעים נפרדים של `DatabaseService` עלולה ליצור נעילות קובץ (`Database is locked`) ובזבוז זיכרון.
2. ה-ViewModel צריך להיות תלוי רק בחוזה **`IDatabaseService`** (הזרקת תלויות דרך הבנאי — `Constructor Injection`).

### בחירת מחזור חיים (`Service Lifetime`) ב-`builder.Services`:
| שיטת רישום | כמה מופעים נוצרים? | למי היא מתאימה ב-MAUI? |
| :--- | :--- | :--- |
| **`AddSingleton<TInterface, TImpl>()`** | **מופע אחד יחיד** לכל חיי האפליקציה. | ✅ **`IDatabaseService` ➡️ `DatabaseService`** — חיבור אסינכרוני מנוהל אחד משותף לכל המסכים! |
| **`AddTransient<T>()`** | **מופע חדש** בכל בקשה/ניווט. | ✅ **ViewModels ו-Pages** (`TeachersViewModel`, `TeachersPage` וכו'). |
| **`AddScoped<T>()`** | מופע אחד לכל היקף בקשה. | נפוץ בעיקר בשרתי Web, פחות ב-MAUI. |

### ✍️ משימה 5.0 — רישום שירות המסד ב-`PlayWithSQLITE/MauiProgram.cs`:
פתחו את `MauiProgram.cs` והוסיפו במקום המסומן `TODO (Student - שלב 5)` את השורה:
```csharp
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```
**בדיקה עצמית:** הריצו את הפרויקט (Build & Run ב-Visual Studio או ב-Rider) — אם השורה חסרה, MAUI יזרוק שגיאה `Unable to resolve service for type 'IDatabaseService'`. לאחר ההוספה האפליקציה עולה חלק!

---

## חלק 2: איך בונים פקודה (`ICommand`) ב-ViewModel? (מהמשימה אל הקוד)

לפני שכותבים קוד של פקודה, תמיד מנסחים במילים פשוטות **מה הפקודה אמורה לבצע במסך**:
> **לדוגמה — פקודת הוספת מורה (`AddTeacherCommand`):**  
> *"כשהמשתמש לוחץ על כפתור 'הוסף מורה', המערכת תבדוק ששם מלא ואימייל הוקלדו, תיצור אובייקט `Teacher` חדש, תשמור אותו ב-SQLite דרך `_databaseService.SaveTeacherAsync(...)`, תאפס את שדות הקלט, ותרענן את רשימת המורים במסך."*

כדי לממש כל פקודה כזו ב-**Vanilla C# MVVM**, פועלים לפי **6 צעדים קבועים**:

1. **הגדרת המצביע/מאפיין מטיפוס `ICommand` (לפני האתחול!):**  
   ה-XAML נקשר למאפיין ציבורי בשם זהה (`Command="{Binding AddTeacherCommand}"`), ולכן חייבים להצהיר עליו קודם במחלקה:
   ```csharp
   public ICommand AddTeacherCommand { get; }
   ```
2. **אתחול הפקודה בבנאי (`Command` / `Command<T>`):**  
   בבנאי ה-ViewModel יוצרים מופע `new Command(...)` (ללא פרמטר) או `new Command<Teacher>(...)` (כשהכפתור בתוך רשימה מעביר `CommandParameter="{Binding .}"`):
   ```csharp
   AddTeacherCommand = new Command(async () => await AddTeacherAsync(), CanAddTeacher);
   ```
3. **הגדרת תנאי `CanExecute` ומיפוי מדויק היכן לקרוא ל-`ChangeCanExecute()`:**
   - **מהו תנאי `CanExecute`?** פונקציה בוליאנית (למשל `CanAddTeacher()`) שמחזירה `true` רק כשהכפתור מותר ללחיצה (בדוגמה שלנו: כאשר **גם** `NewFullName` **וגם** `NewEmail` אינם ריקים!).
   - **מתי התנאי משתנה?** בכל פעם שהמשתמש מקליד או מוחק תו ב-`NewFullName` **או** ב-`NewEmail`!
   - **מה עושה `ChangeCanExecute()` והיכן שמים אותו?** הכפתור ב-XAML אינו "מנחש" לבד שמשהו השתנה. קריאה ל-**`((Command)AddTeacherCommand).ChangeCanExecute();`** בתוך ה-Setter של **כל מאפיין שמופיע בתנאי** שולחת לכפתור אירוע `CanExecuteChanged` — והכפתור נדלק או נכבה אוטומטית (בלי צורך ב-`IsEnabled` ידני ב-XAML)!
4. **הגנת `IsLoading` עם `try / finally`:**  
   מונעת לחיצות כפולות ומפעילה את ה-`ActivityIndicator` בזמן העבודה מול המסד (`if (IsLoading) return;`).
5. **קריאה ל-`_databaseService` ועדכון `ObservableCollection<T>`:**  
   מנקים את האוסף (`Teachers.Clear()`) ומוסיפים בלולאת `foreach` את הפריטים החדשים (`Teachers.Add(...)`) כך שה-`CollectionView` מתרענן אוטומטית.
6. **ניקוי שדות קלט ורענון הרשימה:**  
   לאחר `Save` או `Delete`, מאפסים שדות קלט וקוראים מחדש ל-`await Load...Async()`.

---

## חלק 3: דוגמה מלאה מקצה לקצה — `TeachersViewModel.cs` (מסך המורים)

לפניכם המימוש השלם של **`TeachersViewModel.cs`** הכולל את כל 6 הצעדים (הצהרת `ICommand`, אתחול בבנאי, `CanExecute`, `ChangeCanExecute` ב-Setters, `Command<Teacher>` למחיקה, וטעינת קשר `1:N` עם `includeCourses: true`).  
> 🟡 **מקרא צבעים בקוד:** **השורות הצבועות בצהוב** הן השורות שאתם מוסיפים ומשלימים בשלב 5 (לעומת השורות על רקע לבן שכבר מוכנות עבורכם ב-`main`)!

```csharp
public class TeachersViewModel : BaseViewModel
{
    private readonly IDatabaseService _databaseService;
    private string _newFullName = string.Empty;
    private string _newEmail = string.Empty;
    private string _newDepartment = "מדעי המחשב";

    public ObservableCollection<Teacher> Teachers { get; } = new();

    public string NewFullName
    {
        get => _newFullName;
        set
        {
            if (_newFullName == value) return;
            _newFullName = value;
            OnPropertyChanged();
// [NEW-START]
            // NewFullName מופיע בתנאי CanAddTeacher -> לכן חובה לקרוא כאן ל-ChangeCanExecute!
            ((Command)AddTeacherCommand).ChangeCanExecute();
// [NEW-END]
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
// [NEW-START]
            // NewEmail מופיע בתנאי CanAddTeacher -> לכן חובה לקרוא כאן ל-ChangeCanExecute!
            ((Command)AddTeacherCommand).ChangeCanExecute();
// [NEW-END]
        }
    }

// [NEW-START]
    // 1. הצהרת מצביעי הפקודות מטיפוס ICommand (השמות זהים ל-Binding ב-TeachersPage.xaml!)
    public ICommand LoadTeachersCommand { get; }
    public ICommand AddTeacherCommand { get; }
    public ICommand DeleteTeacherCommand { get; }
// [NEW-END]

    public TeachersViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "ניהול סגל מורים";

// [NEW-START]
        // 2. אתחול הפקודות בבנאי
        LoadTeachersCommand = new Command(async () => await LoadTeachersAsync());
        AddTeacherCommand = new Command(async () => await AddTeacherAsync(), CanAddTeacher);
        DeleteTeacherCommand = new Command<Teacher>(async (teacher) => await DeleteTeacherAsync(teacher));
// [NEW-END]
    }

    // 3. תנאי CanExecute: פעיל רק כשיש שם מלא וגם אימייל
    private bool CanAddTeacher() =>
        !string.IsNullOrWhiteSpace(NewFullName) && !string.IsNullOrWhiteSpace(NewEmail);

    // 4. טעינת המורים כולל הקורסים שלהם (קשר 1:N)
    public async Task LoadTeachersAsync()
    {
// [NEW-START]
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            var list = await _databaseService.GetTeachersAsync();
            Teachers.Clear();
            foreach (var item in list)
            {
                var fullTeacher = await _databaseService.GetTeacherByIdAsync(item.Id, includeCourses: true);
                Teachers.Add(fullTeacher ?? item);
            }
        }
        finally
        {
            IsLoading = false;
        }
// [NEW-END]
    }

    private async Task AddTeacherAsync()
    {
// [NEW-START]
        if (!CanAddTeacher()) return;
        var teacher = new Teacher
        {
            FullName = NewFullName.Trim(),
            Email = NewEmail.Trim(),
            Department = NewDepartment.Trim()
        };
        await _databaseService.SaveTeacherAsync(teacher);
        NewFullName = string.Empty;
        NewEmail = string.Empty;
        await LoadTeachersAsync();
// [NEW-END]
    }

    private async Task DeleteTeacherAsync(Teacher? teacher)
    {
// [NEW-START]
        if (teacher == null) return;
        await _databaseService.DeleteTeacherAsync(teacher);
        await LoadTeachersAsync();
// [NEW-END]
    }
}
```

![צילום חי מ-Android Emulator (MAUI DevFlow) — מסך המורים TeachersPage](../Images/TeachersPage-live.png)

---

## חלק 4: מדריך יישומי שלב-אחר-שלב — איך מממשים דפדוף (`Pagination`) ב-`StudentsViewModel`?

התבוננו בצילום החי של מסך התלמידים (`StudentsPage`) למטה: שימו לב שבשורת הדפדוף כתוב **"עמוד 1 מתוך 2 (סה"כ 6 תלמידים)"**, הכפתור **`◀ קודם`** כבוי (אפור) כי אנחנו בעמוד הראשון, והכפתור **`הבא ▶`** דלוק (סגול) כי יש עמוד שני!

![צילום חי מ-Android Emulator (MAUI DevFlow) — מסך התלמידים ודפדוף StudentsPage](../Images/StudentsPage-live.png)

### כך מממשים דפדוף מלא ב-5 צעדים ברורים בתוך `StudentsViewModel.cs`:

#### שלב א' — הכרת משתני המצב (כבר מוכנים ב-`StudentsViewModel`):
- `_pageSize = 5`: כמה תלמידים מציגים בכל עמוד.
- `CurrentPage` (מתחיל ב-`1`): מספר העמוד הנוכחי.
- `TotalStudents`: המספר הכולל של התלמידים במסד (מגיע מ-`GetStudentsCountAsync()`).
- `TotalPages` (מאפיין מחושב): `Math.Max(1, (int)Math.Ceiling((double)TotalStudents / _pageSize))` (למשל `6 / 5 = 1.2` ➡️ עיגול כלפי מעלה = **2 עמודים**!).
- `PageInfo` (מאפיין מחושב): `$"עמוד {CurrentPage} מתוך {TotalPages} (סה\"כ {TotalStudents} תלמידים)"`.

#### שלב ב' — מיפוי תנאי ה-`CanExecute` והיכן לשים `ChangeCanExecute()`:
| פקודה | תנאי `CanExecute` | מתי התנאי משתנה? | באילו Setters חובה לקרוא ל-`ChangeCanExecute()`? |
| :--- | :--- | :--- | :--- |
| **`NextPageCommand`** | `() => CurrentPage < TotalPages` | כשהעמוד משתנה **או** כשמספר התלמידים הכולל משתנה (הוספה/מחיקה) | ב-Setter של **`CurrentPage`** וב-Setter של **`TotalStudents`**! |
| **`PrevPageCommand`** | `() => CurrentPage > 1` | כשהעמוד משתנה **או** כשהטבלה מתרוקנת | ב-Setter של **`CurrentPage`** וב-Setter של **`TotalStudents`**! |

#### שלב ג' — אתחול שתי הפקודות בבנאי:
```csharp
NextPageCommand = new Command(async () => await NextPageAsync(), () => CurrentPage < TotalPages);
PrevPageCommand = new Command(async () => await PrevPageAsync(), () => CurrentPage > 1);
```

#### שלב ד' — חישוב ה-`skip` ושליפת העמוד הנוכחי בתוך `LoadStudentsAsync()`:
1. שולפים תחילה את סך כל התלמידים: `TotalStudents = await _databaseService.GetStudentsCountAsync();` (ה-Setter יעדכן אוטומטית את `TotalPages`, את `PageInfo`, ואת מצב הכפתורים `NextPageCommand` / `PrevPageCommand`!).
2. מחשבים כמה רשומות לדלג (`skip`):
   - עמוד 1: `(1 - 1) * 5 = 0` ➡️ לוקח רשומות 1–5.
   - עמוד 2: `(2 - 1) * 5 = 5` ➡️ מדלג על 5 הראשונות ולוקח את השאר!
   ```csharp
   int skip = (CurrentPage - 1) * _pageSize;
   var list = await _databaseService.GetStudentsAsync(skip, _pageSize);
   ```
3. מנקים את `Students` וממלאים בלולאת `foreach`.

#### שלב ה' — מימוש `NextPageAsync()` ו-`PrevPageAsync()`:
```csharp
private async Task NextPageAsync()
{
    if (CurrentPage < TotalPages)
    {
        CurrentPage++;          // ה-Setter מעדכן PageInfo ומפעיל ChangeCanExecute!
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
```

---

## חלק 5: ✍️ משימתכם המעשית בשלב 5 — השלמת 3 ה-ViewModels הנותרים

לאחר שהטמעתם את `TeachersViewModel` (חלק 3) ואת מנגנון הדפדוף ב-`StudentsViewModel` (חלק 4), **ממשו באופן דומה** את שאר הפקודות ב-3 ה-ViewModels:

### 1. `StudentsViewModel.cs` (השלמה)
- אתחלו בבנאי את `LoadStudentsCommand`, `AddStudentCommand` (עם `CanAddStudent` — בדקו באילו Setters כבר נקרא `ChangeCanExecute()`!), ו-`DeleteStudentCommand` (`Command<Student>`).
- ממשו את `AddStudentAsync()`, את `DeleteStudentAsync(Student? student)` (כולל איפוס `SelectedStudent` ו-`SelectedStudentTranscript` אם נמחק התלמיד הנבחר), ואת `LoadTranscriptForStudentAsync(int studentId)` הקוראת ל-`GetStudentTranscriptAsync` ול-`GetTotalCreditsForStudentAsync`.

### 2. `CoursesViewModel.cs` (מסך הקורסים)
- אתחלו את `LoadCoursesCommand`, `AddCourseCommand` (עם `CanAddCourse`: דורש גם `NewCourseTitle` וגם `SelectedTeacher != null`!), ו-`DeleteCourseCommand` (`Command<Course>`).
- ב-`LoadCoursesAsync()`: טענו תחילה את המורים (`GetTeachersAsync()`) לתוך `Teachers` (עבור ה-`Picker`), ואז את הקורסים עם קשרים (`GetCoursesAsync(includeRelations: true)`) לתוך `Courses`.
- ב-`AddCourseAsync()`: צרו `Course` חדש עם **`TeacherId = SelectedTeacher!.Id`**, שמרו ב-`SaveCourseAsync`, נקו שדות ורעננו.

![צילום חי מ-Android Emulator (MAUI DevFlow) — מסך הקורסים CoursesPage](../Images/CoursesPage-live.png)

### 3. `ReportsViewModel.cs` (מסך הדוחות)
- אתחלו את `LoadReportsCommand`, `SeedDataCommand`, `ClearDataCommand`.
- ב-`LoadReportsAsync()`: קראו ל-`GetCourseGradeSummariesAsync()` ול-`GetStudentDistributionByGradeLevelAsync()`, מלאו את `CourseSummaries` ואת `GradeDistributions`, ועדכנו את `StatusMessage`.
- **טיפ חשוב ב-`SeedDataAsync()`!** ודאו ש-`IsLoading = false;` מתבצע (בבלוק `finally`) **לפני** הקריאה ל-`await LoadReportsAsync();` — אחרת `LoadReportsAsync()` יראה ש-`IsLoading` עדיין `true` וייצא מיד בלי לרענן את המסך!

![צילום חי מ-Android Emulator (MAUI DevFlow) — מסך הדוחות ReportsPage](../Images/ReportsPage-live.png)

---

### ✅ בדיקה עצמית בסיום שלב 5:
1. בצעו **Build Solution** וודאו שאין שגיאות קומפילציה.
2. הפעילו את האפליקציה באמולטור Android (או Windows), היכנסו ללשונית **דוחות**, לחצו **צור נתוני הדגמה (Seed)** ובדקו שכל 4 הלשוניות מתמלאות בנתונים חיים!
3. הריצו את כל בדיקות היחידה ב-**Test Explorer** (Visual Studio) או ב-**Unit Tests** (Rider) וודאו שכולן עוברות בירוק.
4. להשוואה מול הפתרון המוכן: `git checkout step-05-di-and-viewmodels`.

עברו לשלב האחרון: [06-Testing-And-Verification.pdf](./06-Testing-And-Verification.pdf).
