using SportHub.App.Pages;
using SportHub.App.State;

namespace SportHub.App;

public partial class AppShell : Shell
{
    private readonly AppSessionState _sessionState;

    public AppShell(AppSessionState sessionState)
    {
        InitializeComponent();
        _sessionState = sessionState;

        // Detail pages must be registered as routes, NOT as ShellContent in XAML.
        // Shell uses the DI container to resolve these because they are registered there.
        Routing.RegisterRoute("lesson-details", typeof(LessonDetailsPage));
        Routing.RegisterRoute("check-in", typeof(CheckInPage));
        Routing.RegisterRoute("history", typeof(HistoryPage));
        Routing.RegisterRoute("instructor-login", typeof(InstructorLoginPage));
        Routing.RegisterRoute("instructor-lessons", typeof(InstructorLessonListPage));
        Routing.RegisterRoute("instructor-lesson-details", typeof(InstructorLessonDetailsPage));
        Routing.RegisterRoute("instructor-lesson-edit", typeof(InstructorLessonEditPage));
        Routing.RegisterRoute("diagnostics", typeof(DiagnosticsPage));

        _sessionState.Changed += UpdateInstructorTabVisibility;
        UpdateInstructorTabVisibility();
    }

    private void UpdateInstructorTabVisibility()
    {
        InstructorLessonsTab.IsVisible = _sessionState.IsInstructor;
    }
}
