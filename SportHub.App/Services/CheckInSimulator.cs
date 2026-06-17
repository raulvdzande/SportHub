namespace SportHub.App.Services;

/// <summary>
/// Realistische simulator voor GPS en RFID check-in op emulator
/// </summary>
public class CheckInSimulator
{
    // Realistische RFID tags met sporter informatie
    private static readonly Dictionary<string, string> RfidDatabase = new()
    {
        { "E2002F0A1015501A6E040000", "Jan Jansen" },
        { "E2002F0A1015501A6E040001", "Marie Hendriks" },
        { "E2002F0A1015501A6E040002", "Pieter van Dijk" },
        { "E2002F0A1015501A6E040003", "Lisa de Wilde" },
        { "E2002F0A1015501A6E040004", "Tom Bakker" },
        { "E2002F0A1015501A6E040005", "Sandra Kramer" },
        { "E2002F0A1015501A6E040006", "Erik Mertens" },
        { "E2002F0A1015501A6E040007", "Diana Rutten" },
    };

    // Realistische gymnasium locaties in Nederland
    private static readonly Dictionary<string, (double Lat, double Lon, string City)> GymLocations = new()
    {
        { "Gym Amsterdam Central", (52.3696, 4.9052, "Amsterdam") },
        { "Gym Amsterdam Zuid", (52.3599, 4.8866, "Amsterdam") },
        { "Gym Utrecht", (52.0907, 5.1214, "Utrecht") },
        { "Gym Rotterdam", (51.9225, 4.4792, "Rotterdam") },
        { "Gym Den Haag", (52.0705, 4.3007, "Den Haag") },
        { "Gym Eindhoven", (51.4416, 5.4697, "Eindhoven") },
        { "Gym Almelo", (52.3580, 6.6699, "Almelo") },
        { "Gym Groningen", (53.2193, 6.5665, "Groningen") },
        { "Gym Veghel", (51.5395, 5.2706, "Veghel") },
        { "Gym Breda", (51.5750, 4.7839, "Breda") },
    };

    /// <summary>
    /// Simuleert het lezen van een RFID tag met realistische delay
    /// </summary>
    public static async Task<RfidReadResult> SimulateRfidReadAsync()
    {
        // Realistische RFID-lees delay (500-1500ms)
        var delay = new Random().Next(500, 1500);
        await Task.Delay(delay);

        // Random RFID tag selecteren
        var randomTag = RfidDatabase.ElementAt(new Random().Next(RfidDatabase.Count));

        return new RfidReadResult
        {
            RfidTag = randomTag.Key,
            MemberName = randomTag.Value,
            ReadTimeUtc = DateTime.UtcNow,
            SignalStrength = new Random().Next(75, 100), // dBm
            IsSuccessful = true
        };
    }

    /// <summary>
    /// Simuleert het bepalen van GPS locatie met realistische accuracy en delay
    /// </summary>
    public static async Task<GpsReadResult> SimulateGpsReadAsync(string selectedGymName)
    {
        // Realistische GPS acquire delay (2-8 seconden voor eerste lock)
        var delay = new Random().Next(2000, 8000);
        var startTime = DateTime.UtcNow;

        // Progress callback simulatie
        var progressUpdates = new[] { 25, 50, 75 };
        foreach (var progress in progressUpdates)
        {
            await Task.Delay(delay / 4);
        }

        if (!GymLocations.TryGetValue(selectedGymName, out var gymCoords))
        {
            return new GpsReadResult
            {
                IsSuccessful = false,
                ErrorMessage = "Gym niet gevonden in database"
            };
        }

        // Realistische GPS ruis (+/- 5-15 meter)
        var latNoise = (new Random().NextDouble() - 0.5) * 0.00015; // ~15 meter in latitude
        var lonNoise = (new Random().NextDouble() - 0.5) * 0.00015; // ~15 meter in longitude

        return new GpsReadResult
        {
            Latitude = gymCoords.Lat + latNoise,
            Longitude = gymCoords.Lon + lonNoise,
            Accuracy = new Random().Next(5, 15), // meters
            Altitude = new Random().Next(0, 50), // meters
            City = gymCoords.City,
            Timestamp = DateTime.UtcNow,
            SignalStrength = new Random().Next(-100, -50), // dBm (realistic signal levels)
            SatellitesInUse = new Random().Next(8, 20),
            AcquisitionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds,
            IsSuccessful = true
        };
    }

    /// <summary>
    /// Valideert of de GPS locatie dicht genoeg bij de gym is
    /// </summary>
    public static bool ValidateGpsLocation(double latitude, double longitude, double accuracy = 50)
    {
        // Check of we in een redelijke afstand van een gym zijn (50+ meter mag)
        return accuracy <= 50;
    }

    /// <summary>
    /// Haalt alle beschikbare gym locaties op
    /// </summary>
    public static List<string> GetAvailableGymLocations() => GymLocations.Keys.ToList();
}

public class RfidReadResult
{
    public string RfidTag { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public DateTime ReadTimeUtc { get; set; }
    public int SignalStrength { get; set; } // dBm
    public bool IsSuccessful { get; set; }
}

public class GpsReadResult
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Accuracy { get; set; } // meters
    public int? Altitude { get; set; } // meters
    public int? SatellitesInUse { get; set; }
    public int? AcquisitionTime { get; set; } // milliseconds
    public int? SignalStrength { get; set; } // dBm
    public string? City { get; set; }
    public DateTime? Timestamp { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}
