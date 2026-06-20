# SportHub

SportHub is een fitness- en sportschoolbeheersysteem bestaande uit een mobiele app, een webdashboard en een REST API. Leden kunnen lessen reserveren, inchecken bij een locatie en hun abonnement beheren. Instructeurs kunnen hun lessen bekijken en medewerkers kunnen lessen, locaties en leden beheren.

## Functionaliteiten

### Mobiele app (.NET MAUI)

* Inloggen voor leden en instructeurs
* Beschikbare lessen bekijken
* Lessen reserveren en annuleren
* Wachtlijst bij volle lessen
* Inchecken via GPS, RFID of handmatig
* Pushmeldingen ontvangen
* Profiel beheren inclusief profielfoto
* Abonnementen bekijken en betalen via Stripe
* Historie van reserveringen en check-ins bekijken

### Webdashboard (Blazor)

* Workouts beheren
* Locaties beheren
* Lessen plannen en aanpassen
* Leden beheren
* Overzicht van lessen voor instructeurs

### API (.NET)

* REST API met JWT-authenticatie
* Rolgebaseerde autorisatie
* Koppeling met MySQL via Entity Framework Core
* Stripe-integratie voor betalingen

---

## Gebruikte technieken

### Backend

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* MySQL
* JWT Authentication
* Stripe

### Mobiele app

* .NET MAUI
* MVVM
* Geolocation API
* Secure Storage

### Webdashboard

* Blazor Server
* Bootstrap

---

## Vereisten

Voor het draaien van het project zijn de volgende onderdelen nodig:

* .NET 10 SDK
* MySQL Server 8.0 of hoger
* Android Emulator of Android-toestel
* Visual Studio 2022

---

## Installatie

### Repository clonen

```bash
git clone <repository-url>
cd SportHub
```

### Database aanmaken

```sql
CREATE DATABASE SportHub;
```

Pas vervolgens de connection string aan in:

`SportHub.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=SportHub;user=root;password=your_password"
  }
}
```

### Database migraties uitvoeren

```bash
cd SportHub.API
dotnet ef database update
```

---

## Applicatie starten

### API

```bash
cd SportHub.API
dotnet run
```

### Webdashboard

```bash
cd SportHub.Web
dotnet run
```

### Mobiele app

```bash
cd SportHub.App
dotnet maui run -f net10.0-android
```

---

## Testaccounts

### Lid

Email: member@test.com 
Password: Password123!

### Instructeur

Email: instructor@test.com
Password: Password123!

---

## Belangrijkste API-endpoints

### Authenticatie

```http
POST /api/auth/member/login
POST /api/auth/member/register
POST /api/auth/instructor/login
```

### Lessen

```http
GET    /api/lessons
GET    /api/lessons/{id}
POST   /api/lessons
PUT    /api/lessons/{id}
DELETE /api/lessons/{id}
```

### Reserveringen

```http
GET    /api/reservations/my
POST   /api/reservations
DELETE /api/reservations/{id}
```

### Check-ins

```http
POST /api/checkins
GET  /api/checkins/history
```

---

## Projectstructuur

Het project is opgedeeld in vier losse onderdelen:

SportHub.API
Backend van de applicatie. Bevat de API en alles rondom data en businesslogica.
Controllers (API endpoints)
Services (logica)
Data (DbContext en databaseconfiguratie)
Models (entiteiten)
Program.cs (startup en configuratie)
    
SportHub.App
Mobiele applicatie (MAUI).
Pages (schermen)
ViewModels (MVVM logica)
Services (communicatie met API)
Models (lokale modellen)

    
SportHub.Web
Webdashboard voor beheer.
Pages (Blazor pagina’s)
Components (herbruikbare UI componenten)
Services (API calls en logica)

SportHub.Shared
  Gedeelde code tussen de projecten.
    DTO’s (data die gedeeld wordt tussen API, app en web)

---

## Database

Belangrijkste tabellen:

* Members
* Instructors
* Locations
* Workouts
* Lessons
* LessonReservations
* CheckIns
* MemberSubscriptions
* Notifications

---

## Stripe testgegevens

### Succesvolle betaling

```text
4242 4242 4242 4242
```

### Geweigerde betaling

```text
4000 0000 0000 0002
```

Gebruik een toekomstige vervaldatum en een willekeurige CVC.

---

## Ontwikkeling

Tests uitvoeren:

```bash
dotnet test
```

Nieuwe migratie aanmaken:

```bash
dotnet ef migrations add NaamVanMigratie
```

Database bijwerken:

```bash
dotnet ef database update
```

---

## Beveiliging

* Wachtwoorden worden gehasht opgeslagen
* JWT-authenticatie voor API-toegang
* HTTPS voor alle API-aanroepen
* Rolgebaseerde autorisatie
* Veilige opslag van tokens in de mobiele app
