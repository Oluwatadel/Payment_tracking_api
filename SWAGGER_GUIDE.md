# Swagger/OpenAPI Documentation Guide

## Overview

The Payment Tracker API now includes **full Swagger/OpenAPI documentation** with an interactive UI. This guide explains how to access, use, and understand the API documentation.

---

## Accessing Swagger UI

### During Development

When running the backend in **Development mode**, Swagger UI is automatically enabled:

```bash
cd backend
dotnet run
```

Then open your browser and navigate to:

```
http://localhost:5000/swagger
```

or

```
http://localhost:5001/swagger  (if HTTPS)
```

### In Production

By default, Swagger is **disabled in Production** for security reasons. To enable it in production (not recommended):

Edit `Program.cs` and modify the Swagger middleware configuration to always enable it.

---

## Swagger UI Features

### 1. API Explorer

The Swagger UI provides:
- **Interactive documentation** of all endpoints
- **Request/Response examples** for each endpoint
- **Parameter descriptions** and validation rules
- **Response status codes** (200, 401, 404, 500, etc.)

### 2. Try It Out

You can **test API endpoints directly** from Swagger UI:

1. Click on any endpoint to expand it
2. Click **"Try it out"** button
3. Fill in required parameters
4. Click **"Execute"** to make the request
5. View the response, headers, and curl command

### 3. Authentication

The API uses **JWT (Bearer) tokens** for authentication.

#### How to Authenticate:

1. **Login first** using the `POST /api/auth/login` endpoint
2. Copy the **JWT token** from the response
3. Click the **"Authorize"** button at the top of Swagger UI
4. Paste your token in the format: `Bearer <your-token>`
5. Click **"Authorize"** in the dialog
6. Now all subsequent requests will include your token

#### Example Login Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "phoneNumber": "1234567890",
    "role": "User"
  }
}
```

---

## API Endpoints Overview

### Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---|
| POST | `/api/auth/login` | Login with username and password | No |
| GET | `/api/auth/profile` | Get current user's profile | Yes |

### Users (`/api/users`)

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---|---|
| GET | `/api/users/me` | Get current user's profile | Yes | User/Admin |
| GET | `/api/users/{id}` | Get user by ID | Yes | Admin |
| GET | `/api/users` | List all users | Yes | Admin |
| PUT | `/api/users/{id}` | Update user | Yes | Admin |
| DELETE | `/api/users/{id}` | Delete user | Yes | Admin |
| GET | `/api/users/{id}/account` | Get user's account details | Yes | User/Admin |
| PUT | `/api/users/{id}/account` | Update user's account | Yes | Admin |
| GET | `/api/users/{id}/payments` | Get user's payments | Yes | User/Admin |

### Payments (`/api/payments`)

| Method | Endpoint | Description | Auth Required | Role |
|--------|----------|-------------|---|---|
| GET | `/api/payments` | Get all payments (or own payments) | Yes | User/Admin |
| POST | `/api/payments` | Create a new payment | Yes | Admin |
| GET | `/api/payments/{id}` | Get payment by ID | Yes | Admin/Owner |
| PUT | `/api/payments/{id}` | Update payment | Yes | Admin |
| DELETE | `/api/payments/{id}` | Delete payment | Yes | Admin |
| POST | `/api/payments/{userId}/clear` | Clear user's balance | Yes | Admin |

---

## Common Request Examples

### 1. Login

**Request:**
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "phoneNumber": "1234567890",
    "role": "User"
  }
}
```

### 2. Get Current User Profile

**Request:**
```bash
GET /api/users/me
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "john_doe",
  "phoneNumber": "1234567890",
  "role": "User"
}
```

### 3. Get User's Account Details

**Request:**
```bash
GET /api/users/1/account
Authorization: Bearer <admin-token>
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "bankName": "ABC Bank",
  "accountNumber": "1234567890",
  "balance": 5000.00,
  "totalPaid": 25000.00
}
```

### 4. Create a Payment (Admin)

**Request:**
```bash
POST /api/payments
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "userId": 1,
  "amount": 500.00,
  "bankName": "ABC Bank",
  "referenceNumber": "REF123456"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "userId": 1,
  "amount": 500.00,
  "dateOfPayment": "2024-06-09T12:30:00Z",
  "bankName": "ABC Bank",
  "referenceNumber": "REF123456"
}
```

### 5. Clear User's Balance (Admin)

**Request:**
```bash
POST /api/payments/1/clear
Authorization: Bearer <admin-token>
```

**Response (200 OK):**
```json
{
  "message": "Balance cleared successfully",
  "userId": 1,
  "clearedAmount": 25000.00,
  "newBalance": 0.00
}
```

---

## HTTP Status Codes

The API uses standard HTTP status codes:

| Code | Meaning | Example |
|------|---------|---------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid request body or parameters |
| 401 | Unauthorized | Missing or invalid JWT token |
| 403 | Forbidden | User lacks required permissions |
| 404 | Not Found | Resource not found |
| 500 | Server Error | Unexpected server error |

---

## Error Responses

All error responses follow this format:

```json
{
  "error": "Description of what went wrong"
}
```

### Examples:

**Missing Authentication:**
```json
{
  "error": "User not authenticated"
}
```

**Invalid Credentials:**
```json
{
  "error": "Invalid username or password"
}
```

**Insufficient Permissions:**
```json
{
  "error": "Insufficient permissions"
}
```

---

## Swagger JSON Export

You can export the full OpenAPI specification in JSON format:

```
GET http://localhost:5000/swagger/v1/swagger.json
```

This is useful for:
- Generating client SDKs
- Importing into other API tools (Postman, Insomnia, etc.)
- Version control and documentation
- Integration with CI/CD pipelines

---

## Using with Postman

To use the API with Postman:

1. Open Postman
2. Click **Import** → **Link**
3. Paste: `http://localhost:5000/swagger/v1/swagger.json`
4. Click **Continue** and then **Import**
5. Postman will import all endpoints automatically
6. Create an environment variable `token` with your JWT token
7. Use `{{token}}` in the Authorization header

---

## Best Practices

### 1. **Always Include Authorization Header**
```
Authorization: Bearer <your-jwt-token>
```

### 2. **Check Content-Type**
All requests should include:
```
Content-Type: application/json
```

### 3. **Validate Input**
- Username: Required, alphanumeric
- Phone: Required, valid phone format
- Amount: Required, positive number
- Bank Name: Required, string
- Reference Number: Optional, string

### 4. **Handle Errors Gracefully**
Always check the response status code and handle errors appropriately in your client.

### 5. **Secure Your Token**
- Store JWT tokens securely
- Don't commit tokens to version control
- Use HTTPS in production
- Rotate tokens regularly

---

## Troubleshooting

### Issue: "Swagger page not found"
- **Solution**: Ensure you're running the app in **Development mode**: `dotnet run`
- Check the URL: `http://localhost:5000/swagger`

### Issue: "Authorize button not working"
- **Solution**: Copy the entire token including `Bearer ` prefix, or just paste the token and let Swagger add the prefix.

### Issue: "401 Unauthorized" errors
- **Solution**: Your token has expired or is invalid. Login again to get a new token.

### Issue: "403 Forbidden" errors
- **Solution**: Your role doesn't have permission for this operation. Check if you need Admin role.

---

## Additional Resources

- [OpenAPI 3.0 Specification](https://spec.openapis.org/oas/v3.0.3)
- [Swashbuckle.AspNetCore Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [JWT Authentication Guide](https://jwt.io/)
- [REST API Best Practices](https://restfulapi.net/)

---

## Support

For issues or questions about the API:
1. Check the API_DOCUMENTATION.md file
2. Review the Swagger UI documentation
3. Contact: support@paymenttracker.com
