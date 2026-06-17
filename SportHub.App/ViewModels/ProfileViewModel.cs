using System.IO;
using System.Windows.Input;
using SportHub.App.Helpers;
using SportHub.App.Services.Api;
using SportHub.App.Services.Auth;
using SportHub.App.State;
using SportHub.Shared.DTOs.Members;

namespace SportHub.App.ViewModels;

public class ProfileViewModel : ViewModelBase
{
    private readonly IMembersApiClient _membersApi;
    private readonly IAuthApiService   _authService;
    private readonly AppSessionState   _sessionState;

    private static readonly string PhotoFolder =
        Path.Combine(FileSystem.Current.AppDataDirectory, "profile_photos");

    private static readonly string TempPhotoPath =
        Path.Combine(FileSystem.Current.CacheDirectory, "pending_profile.jpg");

    public ProfileViewModel(IMembersApiClient membersApi, IAuthApiService authService, AppSessionState sessionState)
    {
        _membersApi   = membersApi;
        _authService  = authService;
        _sessionState = sessionState;

        SaveCommand      = new Command(async () => await SaveAsync(),      () => !IsBusy);
        PickPhotoCommand = new Command(async () => await PickPhotoAsync(), () => !IsBusy);
        TakePhotoCommand = new Command(async () => await TakePhotoAsync(), () => !IsBusy);
        LogoutCommand    = new Command(async () => await LogoutAsync());
    }

    public ICommand SaveCommand      { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand LogoutCommand    { get; }

    private string _firstName = string.Empty;
    public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }

    private string _lastName = string.Empty;
    public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _username = string.Empty;
    public string Username { get => _username; set => SetProperty(ref _username, value); }

    private string _phoneNumber = string.Empty;
    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

    private ImageSource? _photoSource;
    public ImageSource? PhotoSource { get => _photoSource; set => SetProperty(ref _photoSource, value); }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            SetProperty(ref _isBusy, value);
            ((Command)SaveCommand).ChangeCanExecute();
            ((Command)PickPhotoCommand).ChangeCanExecute();
            ((Command)TakePhotoCommand).ChangeCanExecute();
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private bool _isSuccess;
    public bool IsSuccess { get => _isSuccess; set => SetProperty(ref _isSuccess, value); }

    private bool _hasPendingPhoto;

    public async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            // Try to get fresh data from API
            var member = await _membersApi.GetCurrentAsync();

            // Fall back to cached session data if API fails
            var source = member ?? _sessionState.CurrentMember;

            if (source is not null)
            {
                FirstName   = source.FirstName   ?? string.Empty;
                LastName    = source.LastName    ?? string.Empty;
                Email       = source.Email       ?? string.Empty;
                Username    = source.Username    ?? string.Empty;
                PhoneNumber = source.PhoneNumber ?? string.Empty;
            }

            // Load photo — works even if API was unavailable
            var memberId = source?.Id;
            if (memberId.HasValue)
                TryLoadPhoto(memberId.Value, source?.ProfilePhotoUrl);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profile: {ex.Message}";
            IsSuccess = false;

            // Still try to load photo from session cache
            if (_sessionState.CurrentMember is { } cached)
            {
                var m = cached;
                FirstName   = m.FirstName   ?? string.Empty;
                LastName    = m.LastName    ?? string.Empty;
                Email       = m.Email       ?? string.Empty;
                Username    = m.Username    ?? string.Empty;
                PhoneNumber = m.PhoneNumber ?? string.Empty;
                TryLoadPhoto(m.Id, m.ProfilePhotoUrl);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void TryLoadPhoto(Guid memberId, string? apiPhotoUrl)
    {
        // Pending (not yet saved) photo takes priority
        if (_hasPendingPhoto && File.Exists(TempPhotoPath))
        {
            PhotoSource = ImageSource.FromFile(TempPhotoPath);
            return;
        }

        // Locally saved photo (persists across restarts)
        var localPath = GetLocalPhotoPath(memberId);
        if (File.Exists(localPath))
        {
            PhotoSource = ImageSource.FromFile(localPath);
            return;
        }

        // API URL as last resort
        if (!string.IsNullOrWhiteSpace(apiPhotoUrl))
            PhotoSource = PhotoUrlHelper.ToImageSource(apiPhotoUrl);
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Select photo"
            });
            if (result is null) return;
            await SetPendingPhotoAsync(result.FullPath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error selecting photo: {ex.Message}";
            IsSuccess = false;
        }
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                StatusMessage = "Camera access denied.";
                IsSuccess = false;
                return;
            }

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                StatusMessage = "Camera not available on this device.";
                IsSuccess = false;
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return;
            await SetPendingPhotoAsync(photo.FullPath);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Camera error: {ex.Message}";
            IsSuccess = false;
        }
    }

    private async Task SetPendingPhotoAsync(string sourcePath)
    {
        using var src  = File.OpenRead(sourcePath);
        using var dest = File.Create(TempPhotoPath);
        await src.CopyToAsync(dest);
        _hasPendingPhoto = true;
        PhotoSource = ImageSource.FromFile(TempPhotoPath);
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            StatusMessage = "First name and last name are required.";
            IsSuccess = false;
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            Stream? photoStream = null;
            if (_hasPendingPhoto && File.Exists(TempPhotoPath))
                photoStream = File.OpenRead(TempPhotoPath);

            var req = new UpdateMemberProfileRequestDto
            {
                Email       = Email,
                FirstName   = FirstName.Trim(),
                LastName    = LastName.Trim(),
                Username    = string.IsNullOrWhiteSpace(Username)    ? null : Username.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim()
            };

            var updated = await _membersApi.UpdateCurrentAsync(req, photoStream, "profile.jpg", "image/jpeg");
            photoStream?.Dispose();

            if (updated is not null)
            {
                // Save photo locally keyed by memberId
                if (_hasPendingPhoto && File.Exists(TempPhotoPath))
                {
                    Directory.CreateDirectory(PhotoFolder);
                    var localPath = GetLocalPhotoPath(updated.Id);
                    File.Copy(TempPhotoPath, localPath, overwrite: true);
                    File.Delete(TempPhotoPath);
                    PhotoSource = ImageSource.FromFile(localPath);
                }

                _hasPendingPhoto = false;
                _sessionState.SetSession(_sessionState.AccessToken ?? string.Empty, updated);
                StatusMessage = "Profile saved successfully!";
                IsSuccess = true;
            }
            else
            {
                StatusMessage = "Save failed. Check your connection.";
                IsSuccess = false;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving profile: {ex.Message}";
            IsSuccess = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        _hasPendingPhoto = false;
        await _authService.LogoutAsync();
        if (Application.Current is App app)
            app.ShowLogin();
    }

    private static string GetLocalPhotoPath(Guid memberId) =>
        Path.Combine(PhotoFolder, $"{memberId}.jpg");
}
