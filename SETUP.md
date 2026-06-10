# C# ASP.NET Core Backend Setup

## Prerequisites

- .NET 8.0 SDK or higher
- PostgreSQL database (or Neon PostgreSQL)
- Git

## Installation

### 1. Install .NET SDK

Download and install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

### 2. Clone or Extract Project

```bash
cd backend
```

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Environment Setup

Create a `.env` file in the backend directory:

```bash
cp .env.example .env
```

Edit `.env` and add your configuration:

```
DATABASE_URL=postgresql://username:password@localhost:5432/payment_tracker
JWT_SECRET=your-super-secret-jwt-key-change-in-production
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000
```

### 5. Create Database

The app will automatically create the database schema on first run using Entity Framework migrations.

However, you can manually create the database:

```bash
# Using PostgreSQL CLI
psql -U postgres
CREATE DATABASE payment_tracker;
```

### 6. Run Migrations

```bash
dotnet ef database update
```

### 7. Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger/index.html`

## Project Structure

```
backend/
├── Models/                 # Data models (User, Account, Payment)
├── DTOs/                   # Data Transfer Objects
├── Services/               # Business logic services
├── Controllers/            # API endpoints
├── Data/                   # Entity Framework DbContext
├── Middleware/             # Custom middleware (JWT)
├── Program.cs              # Application startup
├── PaymentTracker.csproj   # Project file
└── appsettings.json        # Configuration
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login user
- `GET /api/auth/profile` - Get user profile

### Users (Admin Only)
- `GET /api/users` - Get all users
- `POST /api/users` - Create user
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### User Self
- `GET /api/users/me` - Get current user
- `POST /api/users/me/account` - Create account
- `GET /api/users/me/account` - Get account
- `PUT /api/users/me/account` - Update account
- `POST /api/users/me/payments` - Add payment
- `GET /api/users/me/payments` - Get payment history

### Payments (Admin Only)
- `GET /api/payments` - Get all payments
- `GET /api/payments/{id}` - Get payment by ID
- `POST /api/payments/user/{userId}` - Add payment for user
- `PUT /api/payments/{id}` - Update payment
- `DELETE /api/payments/{id}` - Delete payment
- `POST /api/payments/user/{userId}/clear` - Clear user payments

## Testing

### Using Swagger UI
1. Navigate to `http://localhost:5000/swagger/index.html`
2. Click on any endpoint to expand it
3. Click "Try it out" and fill in parameters
4. Click "Execute" to test

### Using cURL

Login:
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

Get Current User (replace TOKEN with actual token):
```bash
curl -X GET "http://localhost:5000/api/users/me" \
  -H "Authorization: Bearer TOKEN"
```

## Database

### PostgreSQL Connection

Using `psql`:
```bash
psql -h localhost -U postgres -d payment_tracker
```

View tables:
```sql
\dt
```

View users:
```sql
SELECT * FROM "Users";
```

## Troubleshooting

### Port Already in Use
Change the port in `Program.cs` or `.env`:
```
ASPNETCORE_URLS=http://localhost:5002
```

### Database Connection Error
1. Check PostgreSQL is running
2. Verify connection string in `.env`
3. Ensure database exists: `CREATE DATABASE payment_tracker;`

### Migrations Error
```bash
# Remove pending migrations
dotnet ef database update 0

# Re-apply migrations
dotnet ef database update
```

### JWT Secret Error
Ensure `JWT_SECRET` is set in `.env` file

## Development

### Enable HTTPS Locally
```bash
dotnet dev-certs https --trust
```

### Hot Reload
```bash
dotnet watch run
```

### View Logs
```bash
dotnet run --verbose
```

## Production Deployment

1. Set environment variables (no .env file)
2. Build release:
   ```bash
   dotnet publish -c Release
   ```
3. Run published app:
   ```bash
   dotnet PaymentTracker.dll
   ```

## Dependencies

- **Microsoft.AspNetCore.OpenApi** - OpenAPI support
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI UI
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider for EF Core
- **Microsoft.EntityFrameworkCore.Tools** - EF Core tools
- **System.IdentityModel.Tokens.Jwt** - JWT token handling
- **BCrypt.Net-Next** - Password hashing
- **DotEnv.Net** - Environment variable loading

## Security Notes

- Change `JWT_SECRET` in production to a strong random value
- Use HTTPS in production
- Implement rate limiting
- Add CORS restrictions for production
- Never commit `.env` file with secrets to version control
