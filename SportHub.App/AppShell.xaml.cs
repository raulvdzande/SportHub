using SportHub.App.Pages;

namespace SportHub.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Detail pages must be registered as routes, NOT as ShellContent in XAML.
        // Shell uses the DI container to resolve these because they are registered there.
        Routing.RegisterRoute("lesson-details", typeof(LessonDetailsPage));
        Routing.RegisterRoute("diagnostics",    typeof(DiagnosticsPage));
    }
}
