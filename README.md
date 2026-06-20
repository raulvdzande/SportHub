# SportHub

A modern fitness gym management platform with mobile app and web dashboard. Members can reserve lessons, check in with GPS/RFID, manage subscriptions, and receive notifications.

## Features

### Mobile App (MAUI)
- **Authentication** - Member & instructor login with JWT
- **Schedule** - Browse available lessons with filters
- **Reservations** - Reserve/cancel lessons with waitlist support
- **Check-In** - Multiple methods:
  - GPS-based check-in (real location detection)
  - RFID card scanning
  - Manual check-in
- **Notifications** - Push notifications for reservations, check-ins
- **Profile** - Member profile with photo upload
- **Subscriptions** - Stripe payment integration for gym memberships
- **History** - View past check-ins and attendance

### Web Dashboard (Blazor)
- **Workout Management** - Create/edit/delete workout types
- **Location Management** - Manage gym locations
- **Lesson Management** - Schedule and manage lessons
- **Member Management** - View and manage members
- **Instructor Portal** - Instructors can view their lessons

### Backend API (.NET 10)
- RESTful API with JWT authentication
- Entity Framework Core with MySQL
- Role-based authorization (Member, Instructor, Staff)
- Real-time data seeding

## Tech Stack

### Backend
- **.NET 10** - Framework
- **ASP.NET Core** - API
- **Entity Framework Core** - ORM
- **MySQL** - Database
- **Stripe** - Payment processing
- **JWT** - Authentication

### Mobile
- **.NET MAUI** - Cross-platform
- **MVVM Architecture**
- **Geolocation** - GPS check-in
- **Local Storage** - Secure token storage

### Web
- **Blazor Server** - UI Framework
- **Bootstrap** - Styling

## Prerequisites

- .NET 10 SDK
- MySQL Server 8.0+
- Android SDK / Emulator (for mobile)
- Visual Studio 2022 or VS Code

## Installation

### 1. Clone Repository
```bash
git clone <repository-url>
cd SportHub
```

### 2. Database Setup

Create MySQL database:
```sql
CREATE DATABASE SportHub CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

Update connection string in `SportHub.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=SportHub;user=root;password=your_password"
  }
}
```

### 3. Apply Migrations
```bash
cd SportHub.API
dotnet ef database update
```

## Running the Application

### API Server
```bash
cd SportHub.API
dotnet run
# Runs on https://localhost:7275
```

### Mobile App
```bash
cd SportHub.App
dotnet maui run -f net10.0-android
# Or for emulator:
dotnet maui run -f net10.0-android -c Debug
```

### Web Dashboard
```bash
cd SportHub.Web
dotnet run
# Runs on https://localhost:7071
```

## GPS Check-In Setup (Android Emulator)

### Enable GPS Simulation
1. Start Android emulator
2. Click **⋮** (Extended controls)
3. Go to **Location**
4. Enter coordinates:
   - **Veghel**: 51.5395, 5.2706
   - **Breda**: 51.5750, 4.7839
5. Click **SET LOCATION**

### Permissions
The app automatically requests location permission on first GPS check-in attempt.

## Test Accounts

### Members
- **Email**: `member@test.com` | **Password**: `Password123!`
- **Email**: `pieter.vandijk@email.com` | **Password**: `SecurePass123!`

### Instructors
- **Email**: `instructor@test.com` | **Password**: `Password123!`
- **Email**: `jan.jansen@gymnasium.nl` | **Password**: `SecurePass123!`

## API Endpoints

### Authentication
```
POST   /api/auth/member/login
POST   /api/auth/member/register
POST   /api/auth/instructor/login
```

### Lessons
```
GET    /api/lessons
GET    /api/lessons/{id}
GET    /api/lessons/mobile
POST   /api/lessons
PUT    /api/lessons/{id}
DELETE /api/lessons/{id}
POST   /api/lessons/generate-recurring
POST   /api/lessons/generate-test-week
```

### Reservations
```
GET    /api/reservations
GET    /api/reservations/my
POST   /api/reservations
DELETE /api/reservations/{id}
```

### Check-Ins
```
POST   /api/checkins
GET    /api/checkins/history
```

### Subscriptions
```
GET    /api/member-subscriptions/me
POST   /api/member-subscriptions
```

### Notifications
```
GET    /api/notifications
GET    /api/notifications/{id}
PUT    /api/notifications/{id}/read
```

## Project Structure

```
SportHub/
├── SportHub.API/              # Backend API
│   ├── Application/           # Services, DTOs
│   ├── Controllers/           # API endpoints
│   ├── Domain/                # Entities, enums
│   ├── Infrastructure/        # Database, seeders
│   └── Program.cs
│
├── SportHub.App/              # Mobile MAUI app
│   ├── Pages/                 # XAML pages
│   ├── ViewModels/            # MVVM logic
│   ├── Services/              # API clients
│   └── MauiProgram.cs
│
├── SportHub.Web/              # Blazor web dashboard
│   ├── Pages/
│   ├── Components/
│   └── Services/
│
└── SportHub.Shared/           # Shared DTOs
```

## Database Schema

### Key Tables
- `Members` - App users
- `Instructors` - Gym instructors
- `Locations` - Gym locations with GPS coords
- `Workouts` - Workout types
- `Lessons` - Scheduled classes
- `LessonReservations` - Member bookings
- `CheckIns` - Attendance records
- `MemberSubscriptions` - Active memberships
- `Notifications` - User notifications

## Payment Integration (Stripe)

### Test Cards
- **Success**: `4242 4242 4242 4242`
- **Declined**: `4000 0000 0000 0002`
- **3D Secure**: `4000 0025 0000 3155`

**Expiry**: Any future date | **CVC**: Any 3 digits

## Development

### Running Tests
```bash
dotnet test
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Revert last migration
dotnet ef database update PreviousMigration
```

### Code Quality
- Follow C# naming conventions (PascalCase for public members)
- Use dependency injection
- Keep controllers thin, logic in services
- Add XML comments for public APIs

## Troubleshooting

### GPS Not Working in Emulator
- Check Extended Controls → Location settings
- Verify `ACCESS_FINE_LOCATION` permission granted
- Ensure emulator API level ≥ 21

### Database Connection Issues
- Verify MySQL is running: `mysql -u root -p`
- Check connection string in `appsettings.json`
- Check firewall (default port 3306)

### Mobile App Won't Start
- Clean build: `dotnet clean && dotnet build`
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Rebuild emulator image

## Security

- JWT tokens stored securely in device storage
- Passwords hashed with PBKDF2
- HTTPS enforced for all API calls
- CORS configured for web clients
- Role-based authorization on all endpoints

## Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Commit changes: `git commit -m "Add feature"`
3. Push to branch: `git push origin feature/your-feature`
4. Create Pull Request

## License

This project is for educational purposes as part of a school assignment.

## Contact

**Developer**: Raúl van der Zande  
**Email**: raulvdzande740@gmail.com

---

**Last Updated**: June 2026  
**Version**: 1.0.0
