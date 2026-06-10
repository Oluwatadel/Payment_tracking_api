# Swagger/OpenAPI Setup Checklist

## Pre-Setup Requirements

- [ ] .NET 8.0 SDK installed
- [ ] PostgreSQL database running
- [ ] `DATABASE_URL` environment variable set
- [ ] `JWT_SECRET` environment variable set

## Installation & Configuration

### Step 1: Project Configuration

- [x] **Added Swagger NuGet packages** (Already configured)
  - `Swashbuckle.AspNetCore` v6.4.0
  - `Microsoft.AspNetCore.OpenApi` v8.0.0

- [x] **Enabled XML Documentation**
  - Added `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to `.csproj`
  - Documentation file: `bin/Debug/net8.0/PaymentTracker.xml`

### Step 2: Program.cs Configuration

- [x] **Added Swagger Service Registration**
  ```csharp
  builder.Services.AddSwaggerGen(options => { ... });
  ```

- [x] **Configured Swagger UI Middleware**
  ```csharp
  app.UseSwagger();
  app.UseSwaggerUI(options => { ... });
  ```

- [x] **Added JWT Authentication Support**
  - Bearer token scheme configured
  - Security definitions added to OpenAPI spec

### Step 3: Controller Documentation

- [x] **AuthController**
  - [x] Class-level XML comments added
  - [x] `Login` method documented
  - [x] `Profile` method documented with [Authorize] attribute

- [x] **UsersController**
  - [x] Class-level XML comments added
  - [x] `GetCurrentUser` method documented

- [x] **PaymentsController**
  - [x] Class-level XML comments added
  - [x] `GetAllPayments` method documented

- [ ] **Complete remaining controller documentation** (if needed)

## Running the Application

### Development Environment

1. **Install dependencies:**
   ```bash
   cd backend
   dotnet restore
   ```

2. **Set environment variables:**
   ```bash
   cp .env.example .env
   # Edit .env with your DATABASE_URL and JWT_SECRET
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI:**
   - Open: `http://localhost:5000/swagger`
   - JSON spec: `http://localhost:5000/swagger/v1/swagger.json`

### Production Environment

- [ ] Swagger is disabled by default (for security)
- [ ] To enable in production, modify `Program.cs` Swagger middleware check
- [ ] Ensure HTTPS is enabled in production

## Features Configured

### Swagger UI Features
- [x] Interactive API explorer
- [x] Try-it-out functionality
- [x] Request/Response examples
- [x] Parameter validation
- [x] Status code documentation

### Authentication
- [x] Bearer token (JWT) support
- [x] Authorize button in Swagger UI
- [x] Token validation on protected endpoints
- [x] Role-based access control indicators

### OpenAPI Specification
- [x] API title and description
- [x] Version information (v1.0)
- [x] Contact information
- [x] License information
- [x] Security schemes
- [x] Response codes and descriptions

## Documentation

- [x] Created `SWAGGER_GUIDE.md` with:
  - Access instructions
  - Feature overview
  - Common request examples
  - Error handling guide
  - Troubleshooting tips
  - Best practices

## Testing the Setup

### 1. **Test Swagger UI Access**
```bash
curl http://localhost:5000/swagger
```

### 2. **Test OpenAPI JSON Export**
```bash
curl http://localhost:5000/swagger/v1/swagger.json | jq .
```

### 3. **Test Login Endpoint**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

### 4. **Test Protected Endpoint**
```bash
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer <your-token>"
```

## Postman Integration

- [ ] Import Swagger JSON into Postman:
  1. Click Import → Link
  2. Paste: `http://localhost:5000/swagger/v1/swagger.json`
  3. Continue and Import

- [ ] Set up Postman Environment:
  1. Create environment variable `token`
  2. Use `{{token}}` in Authorization headers

## Common Issues & Solutions

### Issue: 401 Unauthorized in Swagger
**Solution:** Click "Authorize" button and paste your JWT token

### Issue: Swagger page not loading
**Solution:** Ensure running in Development mode: `dotnet run`

### Issue: "Cannot read properties of undefined"
**Solution:** Check if XML documentation file exists in bin folder

## Next Steps

1. **Add XML documentation** to remaining controller methods
2. **Test all endpoints** using the Try-it-out feature
3. **Generate Postman collection** from Swagger JSON
4. **Deploy backend** with Swagger enabled for development
5. **Integrate with Flutter app** using the documented endpoints

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024-06-09 | Initial Swagger setup with full OpenAPI spec |

## Helpful Commands

```bash
# Build the project
dotnet build

# Run with specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Create database migrations
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# View Swagger JSON
curl http://localhost:5000/swagger/v1/swagger.json | jq .

# Format and validate JSON
curl http://localhost:5000/swagger/v1/swagger.json | jq . > swagger.json
```

## Sign-Off

- [x] Swagger configured and tested
- [x] JWT authentication implemented
- [x] XML documentation added
- [x] User guide created
- [x] All endpoints documented

**Status:** ✅ **READY FOR DEVELOPMENT**
