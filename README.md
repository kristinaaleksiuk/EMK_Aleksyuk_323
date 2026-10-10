# УП.01.01 «Приём и ведение ЭМК» — WPF-приложение (C#, .NET Framework 4.8)

Интерфейс сделан по макету из файла `Макет_WPF_ЭМК.docx`, код — по методичке `УП_01_Метод_указания_2026.docx`.

## Запуск
1. **База данных.** Ваша БД `323_EMK_ALEKSUK` создаётся скриптом `Database/EMK_schema.sql` (ваш скрипт без изменений).
   Затем выполните `Database/EMK_seed_data.sql` — он добавит тестовых пользователей, врачей, пациентов, расписание и приёмы.
   В SSMS создайте диаграмму БД и сохраните её как `DbDiagram` (Шаг 1).
2. **Строка подключения.** В `App.config` уже указаны `.\SQLEXPRESS` и `323_EMK_ALEKSUK`; при другом имени сервера измените `Data Source`.
3. **Переименование проекта** (требование Шага 2: группа + фамилия латиницей). В Visual Studio: Ctrl+Shift+H
   (замена в файлах) `UP01_Surname_EMK` → ваше имя, затем переименуйте папку и `.csproj`.
4. Откройте `.csproj` в Visual Studio 2019/2022, дождитесь восстановления NuGet-пакетов (EntityFramework, Office Interop) и запустите (F5).
5. Для Шага 2 п. 7 замените `app.ico` на свою иконку (Проект → Свойства → Приложение → Значок), если нужна другая.

## Тестовые учётные записи
| Логин | Пароль | Роль |
|---|---|---|
| admin | admin123 | Admin |
| user1 | user1234 | User |
| user2 | user2345 | User |

## Соответствие шагам методички
| Шаг | Где |
|---|---|
| 1. БД | `Database/EMK_schema.sql`, `Database/EMK_seed_data.sql` |
| 2. Главное окно | `MainWindow.xaml(.cs)`: Grid 45/*/45, Frame `MainFrame`, «Назад», дата и время, подтверждение закрытия |
| 3. Словари стилей | `Dictionary.xaml` (основная тема), `Dictionary2.xaml` (вторая), общие стили в `Styles.xaml`; переключение — кнопка «Сменить тему» |
| 4–5. Авторизация и капча | `Pages/AuthPage` |
| 6–7. Регистрация, SHA1 | `Pages/RegPage`, `Helpers/Helpers.cs` (`Security`) |
| 8. Страница пользователя | `Pages/UserPage` |
| 9. Страница администратора | `Pages/AdminPage` |
| 10. CRUD | `UsersPage`/`AddUserPage`, `PatientsPage`/`AddPatientPage`, `AppointmentsPage`/`AddAppointmentPage` |
| 11. Смена пароля | `Pages/ChangePasswordPage` |
| 12. Диаграмма | `Pages/ChartsPage` (Chart в WindowsFormsHost) |
| 13–14. Word, Excel (+PDF) | `Pages/ChartsPage.xaml.cs` |

  потому что в вашей БД эти поля обязательны. Администратор дополняет их на странице «Пациенты».
