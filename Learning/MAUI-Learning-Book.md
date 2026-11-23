# ספר הלמידה: מסד נתונים מקומי SQLite מרובה-טבלאות ב-.NET MAUI

ספר למידה זה מלווה את פרויקט `PlayWithSQLITE`. הוא מרכז את עקרונות הנדסת התוכנה, תכנון ERD, עיצוב הנתונים, פקודות ה-SQL, הזרקת תלויות (DI), חיבור MVVM ובדיקות יחידה ב-xUnit v3 שנלמדו במסגרת פרויקט הבגרות ברמת 5 יחידות לימוד.

---

## 🧪 הקדמה מעשית: הכרת בדיקות יחידה (`xUnit v3`)
- **מדריך מפורט למתחילים:** [00b-How-To-Run-Tests.pdf](../Docs/00b-How-To-Run-Tests.pdf) — מבנה קובץ בדיקה (`Arrange–Act–Assert`, `IAsyncLifetime` עם `ValueTask`), הסרת `Skip` בהדרגה, הוספת בדיקה חדשה שלב-אחר-שלב, הרצה ב-Visual Studio (`Test Explorer`) וב-Rider (`Unit Tests`), וניתוח הודעות שגיאה.

---

## 📚 פרק 1: התקנה ויזואלית, תרשים ERD, קשרי גומלין, `CascadeOperation` ומודלים
- **מה נבנה:** התקנת החבילות הרשמיות והיציבות (`Stable` בלבד, ללא Prerelease) `sqlite-net-pcl`, `SQLiteNetExtensions`, `SQLiteNetExtensions.Async` ועדכון `Newtonsoft.Json` לגרסה היציבה האחרונה (`13.0.4`) בכל הפרויקטים דרך הממשק הויזואלי ב-Visual Studio וב-Rider; תכנון תרשים **ERD** ל-4 טבלאות (`Teachers`, `Courses`, `Students`, `Enrollments`); לימוד מתוך דוגמת ספרייה דיגיטלית (`Author`, `Book`, `Reader`, `BookLoan`); והגדרת הדקורציות והקשרים בשני צדי הקשר (`1:N` ו-`M:N`).
- **מדריך מפורט:** [01-Nuget-Packages-And-Models.pdf](../Docs/01-Nuget-Packages-And-Models.pdf).

---

## 📚 פרק 2: מחלקת `Constants`, הבנת `IDatabaseService`, אתחול פנימי (`InitDBAsync`) ו-CRUD
- **מה נבנה:** מחלקה סטטית ייעודית `Constants.cs` המרכזת את `DatabaseFilename`, `SQLiteOpenFlags` ו-`DatabasePath`; הבנת החוזה `IDatabaseService` (כולל שמירת האתחול כפרטי ואוטומטי בתוך `DatabaseService` באמצעות `InitDBAsync()`); טבלת הפעולות של `sqlite-net-pcl`; הסבר האופרטור `_database!.` (Null-Forgiving Operator); ומימוש שלב-אחר-שלב של CRUD מלא.
- **מדריך מפורט:** [02-Database-Connection-And-Basic-CRUD.pdf](../Docs/02-Database-Connection-And-Basic-CRUD.pdf).

---

## 📚 פרק 3: עבודה מעשית עם קשרי גומלין בשכבת השירות (`1:N` ו-`M:N`)
- **מה נבנה:** טעינת אובייקטים מקושרים באמצעות `SQLiteNetExtensions.Async` (`GetWithChildrenAsync`, `GetAllWithChildrenAsync`), מניעת כפילות רישום בטבלת הגישור `Enrollment`, ומחיקה מבוקרת המונעת רשומות יתומות (`Orphan Records`).
- **מדריך מפורט:** [03-Table-Relationships.pdf](../Docs/03-Table-Relationships.pdf).

---

## 📚 פרק 4: פירוק שאילתות SQL מתקדמות — סדר ביצוע, `JOIN`, `GROUP BY`, אגרגציות ודפדוף
- **מה נבנה:** סדר הביצוע הלוגי ב-SQL (`FROM` ➡️ `JOIN` ➡️ `WHERE` ➡️ `GROUP BY` ➡️ `HAVING` ➡️ `SELECT` ➡️ `ORDER BY` ➡️ `LIMIT/OFFSET`), הדגמה מהיעד אל טבלאות הבסיס ופירוק שלב-אחר-שלב של `INNER JOIN`, `LEFT JOIN`, `GROUP BY`, `COUNT(e.Id)` מול `COUNT(*)`, `SUM` עם `ExecuteScalarAsync<int>`, ודפדוף עם `OrderBy` + `Skip` + `Take`.
- **מדריך מפורט:** [04-Advanced-Queries-Aggregations.pdf](../Docs/04-Advanced-Queries-Aggregations.pdf).

---

## 📚 פרק 5: רישום ב-DI Container (`MauiProgram.cs`), פקודות `ICommand` ויישום Pagination
- **מה נבנה:** רישום `IDatabaseService` מול `DatabaseService` כ-`Singleton` ב-DI Container; דוגמה מלאה של `TeachersViewModel` (`ICommand`, `CanExecute`, `ChangeCanExecute`, `Command<Teacher>`, הגנת `IsLoading`); מדריך יישומי שלב-אחר-שלב ל-Pagination ב-`StudentsViewModel`; וצילומי מסך חיים מאמולטור Android דרך **MAUI DevFlow**.
- **מדריך מפורט:** [05-DI-And-ViewModel-Commands.pdf](../Docs/05-DI-And-ViewModel-Commands.pdf).

---

## 📚 פרק 6: אימות מלא, בדיקות `xUnit v3` ומפת הענפים
- **מדריך אימות סופי:** [06-Testing-And-Verification.pdf](../Docs/06-Testing-And-Verification.pdf).
- **מדריך המשימה הראשי:** [00-Master-Assignment.pdf](../Docs/Assignment/00-Master-Assignment.pdf).
