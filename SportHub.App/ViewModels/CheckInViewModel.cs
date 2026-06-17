using SportHub.App.Services;
using SportHub.App.Services.Api;
using SportHub.Shared.DTOs.CheckIns;
using SportHub.Shared.DTOs.Lessons;
using System.Windows.Input;

namespace SportHub.App.ViewModels;

public class CheckInViewModel : ViewModelBase
{
    private readonly ICheckInsApiClient _checkInsApi;
    private readonly ILessonsApiClient _lessonsApi;

    // Pre-defined locations for simulator
    private static readonly Dictionary<string, (double Lat, double Lon)> PresetLocations = new()
    {
        { "Gym Amsterdam Central", (52.3696, 4.9052) },
        { "Gym Amsterdam Zuid", (52.3599, 4.8866) },
        { "Gym Utrecht", (52.0907, 5.1214) },
        { "Gym Rotterdam", (51.9225, 4.4792) },
        { "Gym Den Haag", (52.0705, 4.3007) },
        { "Gym Veghel", (51.5395, 5.2706) },
        { "Gym Breda", (51.5750, 4.7839) }
    };

    public CheckInViewModel(ICheckInsApiClient checkInsApi, ILessonsApiClient lessonsApi)
    {
        _checkInsApi = checkInsApi;
        _lessonsApi = lessonsApi;

        CheckInCommand = new Command(async () => await CheckInAsync());
        LoadHistoryCommand = new Command(async () => await LoadHistoryAsync());
        StartRfidScanCommand = new Command(async () => await StartRfidScanAsync());
        StartGpsScanCommand = new Command(async () => await StartGpsScanAsync());
        CancelScanCommand = new Command(() => CancelScan());
    }

    private MobileLessonDetailsDto? _currentLesson;
    public MobileLessonDetailsDto? CurrentLesson { get => _currentLesson; set => SetProperty(ref _currentLesson, value); }

    private string _method = "Manual";
    public string Method { get => _method; set => SetProperty(ref _method, value); }

    private string _rfidTag = string.Empty;
    public string RfidTag { get => _rfidTag; set => SetProperty(ref _rfidTag, value); }

    private string _rfidMemberName = string.Empty;
    public string RfidMemberName { get => _rfidMemberName; set => SetProperty(ref _rfidMemberName, value); }

    private int _rfidSignalStrength;
    public int RfidSignalStrength { get => _rfidSignalStrength; set => SetProperty(ref _rfidSignalStrength, value); }

    private double _gpsLatitude;
    public double GpsLatitude { get => _gpsLatitude; set => SetProperty(ref _gpsLatitude, value); }

    private double _gpsLongitude;
    public double GpsLongitude { get => _gpsLongitude; set => SetProperty(ref _gpsLongitude, value); }

    private int _gpsAccuracy;
    public int GpsAccuracy { get => _gpsAccuracy; set => SetProperty(ref _gpsAccuracy, value); }

    private int _gpsSatellites;
    public int GpsSatellites { get => _gpsSatellites; set => SetProperty(ref _gpsSatellites, value); }

    private int _gpsAcquisitionTime;
    public int GpsAcquisitionTime { get => _gpsAcquisitionTime; set => SetProperty(ref _gpsAcquisitionTime, value); }

    private string _gpsCity = string.Empty;
    public string GpsCity { get => _gpsCity; set => SetProperty(ref _gpsCity, value); }

    private bool _isScanning;
    public bool IsScanning { get => _isScanning; set => SetProperty(ref _isScanning, value); }

    private int _scanProgress;
    public int ScanProgress { get => _scanProgress; set => SetProperty(ref _scanProgress, value); }

    private string _selectedGym = string.Empty;
    public string SelectedGym { get => _selectedGym; set => SetProperty(ref _selectedGym, value); }

    private List<string> _availableGyms = [];
    public List<string> AvailableGyms { get => _availableGyms; set => SetProperty(ref _availableGyms, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set { SetProperty(ref _isBusy, value); ((Command)CheckInCommand).ChangeCanExecute(); } }

    private string _error = string.Empty;
    public string Error { get => _error; set => SetProperty(ref _error, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    private CancellationTokenSource? _scanCancellation;

    public ICommand CheckInCommand { get; }
    public ICommand LoadHistoryCommand { get; }
    public ICommand StartRfidScanCommand { get; }
    public ICommand StartGpsScanCommand { get; }
    public ICommand CancelScanCommand { get; }

    public async Task LoadLessonAsync(Guid lessonId)
    {
        IsBusy = true;
        Error = string.Empty;
        try
        {
            CurrentLesson = await _lessonsApi.GetMobileDetailsAsync(lessonId);
            if (AvailableGyms.Count == 0)
            {
                AvailableGyms = CheckInSimulator.GetAvailableGymLocations();
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Timeout — check your network.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartRfidScanAsync()
    {
        if (IsScanning) return;
        if (Method == "Gps") return; // Prevent if GPS is about to scan

        IsScanning = true;
        // Clear GPS results
        GpsLatitude = 0;
        GpsLongitude = 0;
        GpsCity = string.Empty;
        ScanProgress = 0;
        Error = string.Empty;
        StatusMessage = "📡 Reading RFID tag...";
        _scanCancellation = new CancellationTokenSource();

        try
        {
            for (int i = 0; i < 5; i++)
            {
                if (_scanCancellation.Token.IsCancellationRequested) return;
                ScanProgress = (i + 1) * 20;
                await Task.Delay(200);
            }

            var result = await CheckInSimulator.SimulateRfidReadAsync();

            if (result.IsSuccessful)
            {
                RfidTag = result.RfidTag;
                RfidMemberName = result.MemberName;
                RfidSignalStrength = result.SignalStrength;
                Method = "Rfid";
                StatusMessage = $"✅ RFID read: {result.MemberName} (signal: {result.SignalStrength}%)";
            }
            else
            {
                Error = "Failed to read RFID. Try again.";
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "📴 RFID scan cancelled";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsScanning = false;
            ScanProgress = 0;
            _scanCancellation?.Dispose();
        }
    }

    private async Task StartGpsScanAsync()
    {
        if (IsScanning) return;
        if (Method == "Rfid") return; // Prevent if RFID is about to scan

        if (string.IsNullOrWhiteSpace(SelectedGym))
        {
            Error = "Select a gym location";
            return;
        }

        IsScanning = true;
        // Clear RFID results
        RfidTag = string.Empty;
        RfidMemberName = string.Empty;
        RfidSignalStrength = 0;
        ScanProgress = 0;
        Error = string.Empty;
        StatusMessage = "📍 Requesting location permission...";
        _scanCancellation = new CancellationTokenSource();

        try
        {
            // Request location permission
            var permissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    Error = "Location permission denied. Enable in app settings.";
                    return;
                }
            }

            StatusMessage = "📡 Getting your location...";
            ScanProgress = 25;

            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(30)
            }, _scanCancellation.Token);

            if (location == null)
            {
                Error = "Could not determine your location. Check GPS is enabled.";
                return;
            }

            ScanProgress = 75;
            GpsLatitude = location.Latitude;
            GpsLongitude = location.Longitude;
            GpsAccuracy = (int?)location.Accuracy ?? 0;
            GpsSatellites = 0;
            GpsAcquisitionTime = 0;
            GpsCity = await GetCityFromCoordinatesAsync(location.Latitude, location.Longitude);
            Method = "Gps";

            ScanProgress = 100;

            // Validate location matches lesson location
            if (!ValidateGpsLocation())
            {
                Error = $"GPS location incorrect. You are in {GpsCity}, but the lesson is at {CurrentLesson?.LocationName}. Please select the correct location and scan again.";
                GpsLatitude = 0;
                GpsLongitude = 0;
                GpsCity = string.Empty;
                Method = "Manual";
                return;
            }

            StatusMessage = $"✅ Located in {GpsCity} (accuracy: {GpsAccuracy}m)";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "📴 GPS scan cancelled";
        }
        catch (Exception ex)
        {
            Error = $"Location error: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            ScanProgress = 0;
            _scanCancellation?.Dispose();
        }
    }

    private async Task<string> GetCityFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            // ALWAYS determine actual location from GPS coordinates first

            // Try to find nearest gym from available locations
            var nearestGym = FindNearestGym(latitude, longitude);
            if (nearestGym != null)
            {
                return nearestGym;
            }

            // Fallback: use reverse geocoding with city/locality
            var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
            if (placemarks != null)
            {
                var placemark = placemarks.FirstOrDefault();
                if (placemark != null)
                {
                    return !string.IsNullOrEmpty(placemark.Locality)
                        ? placemark.Locality
                        : placemark.AdminArea ?? "Unknown";
                }
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private string? FindNearestGym(double latitude, double longitude)
    {
        if (AvailableGyms.Count == 0) return null;

        var gymCoordinates = new Dictionary<string, (double Lat, double Lon)>
        {
            { "Gym Amsterdam Central", (52.3696, 4.9052) },
            { "Gym Amsterdam Zuid", (52.3599, 4.8866) },
            { "Gym Utrecht", (52.0907, 5.1214) },
            { "Gym Rotterdam", (51.9225, 4.4792) },
            { "Gym Den Haag", (52.0705, 4.3007) },
            { "Gym Veghel", (51.5395, 5.2706) },
            { "Gym Breda", (51.5750, 4.7839) }
        };

        double minDistance = double.MaxValue;
        string? nearestGym = null;

        foreach (var gym in AvailableGyms)
        {
            if (gymCoordinates.TryGetValue(gym, out var coords))
            {
                double distance = CalculateDistance(latitude, longitude, coords.Lat, coords.Lon);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestGym = gym;
                }
            }
        }

        // Only return if within 1km
        return minDistance < 1.0 ? nearestGym : null;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private bool ValidateGpsLocation()
    {
        // If SelectedGym is provided, must match scanned location
        if (!string.IsNullOrEmpty(SelectedGym))
        {
            // Extract city name from "Gym CityName" format
            var selectedCityName = SelectedGym.StartsWith("Gym ", StringComparison.OrdinalIgnoreCase)
                ? SelectedGym.Substring(4).Trim()
                : SelectedGym;

            return selectedCityName.Equals(GpsCity, StringComparison.OrdinalIgnoreCase) ||
                   SelectedGym.Equals(GpsCity, StringComparison.OrdinalIgnoreCase);
        }

        // If SelectedGym not provided, check if scanned location contains lesson location name
        if (CurrentLesson != null && !string.IsNullOrEmpty(CurrentLesson.LocationName))
        {
            // Extract city name from "Gym CityName" format
            var locationCityName = CurrentLesson.LocationName.StartsWith("Gym ", StringComparison.OrdinalIgnoreCase)
                ? CurrentLesson.LocationName.Substring(4).Trim()
                : CurrentLesson.LocationName;

            return GpsCity.Equals(locationCityName, StringComparison.OrdinalIgnoreCase) ||
                   GpsCity.Equals(CurrentLesson.LocationName, StringComparison.OrdinalIgnoreCase) ||
                   GpsCity.Contains(locationCityName, StringComparison.OrdinalIgnoreCase) ||
                   CurrentLesson.LocationName.Contains(GpsCity, StringComparison.OrdinalIgnoreCase);
        }

        // If no SelectedGym and no CurrentLesson location, assume valid
        return true;
    }

    private void CancelScan()
    {
        _scanCancellation?.Cancel();
    }

    private async Task CheckInAsync()
    {
        if (IsBusy || CurrentLesson == null) return;

        if (Method == "Rfid" && string.IsNullOrWhiteSpace(RfidTag))
        {
            Error = "Click 'Start RFID Scan' to read a tag";
            return;
        }

        if (Method == "Gps" && (GpsLatitude == 0 || GpsLongitude == 0))
        {
            Error = "Click 'Start GPS Scan' to determine location";
            return;
        }

        IsBusy = true;
        Error = string.Empty;
        try
        {
            var request = new CreateCheckInRequestDto
            {
                LessonId = CurrentLesson.Id,
                Method = Method,
                DeviceIdentifier = Method == "Rfid" ? RfidTag : null,
                Latitude = Method == "Gps" ? GpsLatitude : null,
                Longitude = Method == "Gps" ? GpsLongitude : null
            };

            var result = await _checkInsApi.CreateAsync(request);
            if (result != null)
            {
                StatusMessage = $"✅ Successfully checked in via {Method}!";
                PushNotificationService.SendNotification("Check In Successful", $"You checked in to {CurrentLesson.WorkoutName} via {Method}");
                await Task.Delay(2000);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                Error = "Failed to process check-in.";
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Timeout — check your network.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadHistoryAsync()
    {
        IsBusy = true;
        Error = string.Empty;
        try
        {
            var history = await _checkInsApi.GetMemberHistoryAsync();
            if (history != null && history.Count > 0)
            {
                StatusMessage = $"📊 {history.Count} check-ins in the last year.";
            }
            else
            {
                StatusMessage = "No check-ins found";
            }
        }
        catch (TaskCanceledException)
        {
            Error = "Timeout — check your network.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
