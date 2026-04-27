using SportHub.Web.Services.Storage;

namespace SportHub.Web.Services.Localization;

public class LanguageService
{
    private const string StorageKey = "sporthub.language";

    private static readonly Dictionary<string, IReadOnlyDictionary<string, string>> Resources =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["nl"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["app.title"] = "SportHub Medewerker",
                ["nav.login"] = "Inloggen",
                ["nav.logout"] = "Uitloggen",
                ["nav.workouts"] = "Workouts",
                ["nav.instructors"] = "Instructeurs",
                ["home.title"] = "Medewerker dashboard",
                ["home.subtitle"] = "Beheer workouts en instructeurs.",
                ["login.title"] = "Inloggen",
                ["login.email"] = "E-mailadres",
                ["login.password"] = "Wachtwoord",
                ["login.submit"] = "Inloggen",
                ["login.error"] = "Inloggen mislukt. Controleer je gegevens.",
                ["workouts.title"] = "Workouts beheren",
                ["workouts.new"] = "Nieuwe workout",
                ["workouts.edit"] = "Workout wijzigen",
                ["workouts.name"] = "Naam",
                ["workouts.description"] = "Beschrijving",
                ["workouts.duration"] = "Duur (minuten)",
                ["workouts.active"] = "Actief",
                ["workouts.save"] = "Opslaan",
                ["workouts.cancel"] = "Annuleren",
                ["workouts.delete"] = "Verwijderen",
                ["workouts.deleted.lessons"] = "Workout verwijderd. Verwijderde lessen:",
                ["instructors.title"] = "Instructeurs toevoegen",
                ["instructors.name"] = "Naam",
                ["instructors.photo"] = "Foto",
                ["instructors.save"] = "Toevoegen",
                ["common.loading"] = "Laden..."
            },
            ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["app.title"] = "SportHub Employee",
                ["nav.login"] = "Login",
                ["nav.logout"] = "Logout",
                ["nav.workouts"] = "Workouts",
                ["nav.instructors"] = "Instructors",
                ["home.title"] = "Employee dashboard",
                ["home.subtitle"] = "Manage workouts and instructors.",
                ["login.title"] = "Login",
                ["login.email"] = "Email",
                ["login.password"] = "Password",
                ["login.submit"] = "Login",
                ["login.error"] = "Login failed. Check your credentials.",
                ["workouts.title"] = "Manage workouts",
                ["workouts.new"] = "New workout",
                ["workouts.edit"] = "Edit workout",
                ["workouts.name"] = "Name",
                ["workouts.description"] = "Description",
                ["workouts.duration"] = "Duration (minutes)",
                ["workouts.active"] = "Active",
                ["workouts.save"] = "Save",
                ["workouts.cancel"] = "Cancel",
                ["workouts.delete"] = "Delete",
                ["workouts.deleted.lessons"] = "Workout deleted. Deleted lessons:",
                ["instructors.title"] = "Add instructors",
                ["instructors.name"] = "Name",
                ["instructors.photo"] = "Photo",
                ["instructors.save"] = "Add",
                ["common.loading"] = "Loading..."
            }
        };

    private readonly BrowserStorageService _storage;

    public LanguageService(BrowserStorageService storage)
    {
        _storage = storage;
    }

    public string CurrentLanguage { get; private set; } = "nl";

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        var storedLanguage = await _storage.GetItemAsync(StorageKey);
        if (!string.IsNullOrWhiteSpace(storedLanguage) && Resources.ContainsKey(storedLanguage))
        {
            CurrentLanguage = storedLanguage;
        }
    }

    public async Task ToggleAsync()
    {
        CurrentLanguage = CurrentLanguage.Equals("nl", StringComparison.OrdinalIgnoreCase) ? "en" : "nl";
        await _storage.SetItemAsync(StorageKey, CurrentLanguage);
        Changed?.Invoke();
    }

    public string T(string key)
    {
        if (Resources.TryGetValue(CurrentLanguage, out var language) && language.TryGetValue(key, out var value))
        {
            return value;
        }

        return key;
    }
}
