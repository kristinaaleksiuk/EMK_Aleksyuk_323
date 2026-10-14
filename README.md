# УП.01.01 «Приём и ведение ЭМК» — WPF-приложение (C#, .NET Framework 4.8)


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


