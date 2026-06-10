# Payment Tracker API Documentation

## Base URL
```
http://localhost:5000/api
https://your-production-domain.com/api
```

## Authentication

All endpoints (except login) require JWT token in Authorization header:
```
Authorization: Bearer <token>
```

## Response Format

### Success Response
```json
{
  "data": { },
  "message": "Success"
}
```

### Error Response
```json
{
  "error": "Error message",
  "statusCode": 400
}
```

---

## Authentication Endpoints

### Login
**POST** `/auth/login`

Login with username and password to receive JWT token.

**Request Body:**
```json
{
  "username": "john_doe",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "userId": 1,
  "username": "john_doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "User"
}
```

**Errors:**
- 400: Username and password are required
- 401: Invalid username or password
- 500: Login failed

---

### Get Profile
**GET** `/auth/profile`

Get current authenticated user's profile.

**Headers:**
```
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

**Errors:**
- 401: User not authenticated
- 404: User not found
- 500: Internal server error

---

## User Endpoints

### Get Current User
**GET** `/users/me`

Get the current authenticated user's details.

**Headers:**
```
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

---

### Get All Users (Admin Only)
**GET** `/users`

Get list of all users. **Admin only.**

**Headers:**
```
Authorization: Bearer <admin-token>
```

**Query Parameters:**
- None

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "username": "john_doe",
    "phoneNumber": "1234567890",
    "role": "User"
  },
  {
    "id": 2,
    "username": "jane_admin",
    "phoneNumber": "0987654321",
    "role": "Admin"
  }
]
```

**Errors:**
- 401: User not authenticated
- 403: Forbidden (not admin)

---

### Get User by ID (Admin Only)
**GET** `/users/{id}`

Get a specific user by ID. **Admin only.**

**Parameters:**
- `id` (path): User ID

**Headers:**
```
Authorization: Bearer <admin-token>
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

**Errors:**
- 404: User not found
- 403: Forbidden (not admin)

---

### Create User (Admin Only)
**POST** `/users`

Create a new user. **Admin only.**

**Headers:**
```
Authorization: Bearer <admin-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "new_user",
  "phoneNumber": "9876543210",
  "password": "securePassword123"
}
```

**Response (201 Created):**
```json
{
  "id": 3,
  "username": "new_user",
  "phoneNumber": "9876543210",
  "role": "User"
}
```

**Errors:**
- 400: Username or phone number already exists
- 403: Forbidden (not admin)
- 500: Failed to create user

---

### Update User (Admin Only)
**PUT** `/users/{id}`

Update a user. **Admin only.**

**Parameters:**
- `id` (path): User ID

**Headers:**
```
Authorization: Bearer <admin-token>
Content-Type: application/json
```

**Request Body (all fields optional):**
```json
{
  "username": "updated_username",
  "phoneNumber": "1111111111",
  "password": "newPassword123"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "updated_username",
  "phoneNumber": "1111111111",
  "role": "User"
}
```

**Errors:**
- 404: User not found
- 403: Forbidden (not admin)

---

### Delete User (Admin Only)
**DELETE** `/users/{id}`

Delete a user. **Admin only.**

**Parameters:**
- `id` (path): User ID

**Headers:**
```
Authorization: Bearer <admin-token>
```

**Response (204 No Content)**

**Errors:**
- 404: User not found
- 403: Forbidden (not admin)

---

## Account Endpoints

### Get Current User Account
**GET** `/users/me/account`

Get the current user's bank account details.

**Headers:**
```
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "id": 1,
  "userId": 1,
  "bankName": "Bank of America",
  "accountNumber": "123456789",
  "accountHolder": "John Doe",
  "balance": 5000.00
}
```

**Errors:**
- 401: User not authenticated
- 404: Account not found

---

### Create Account
**POST** `/users/me/account`

Create a bank account for current user.

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "bankName": "Bank of America",
  "accountNumber": "123456789",
  "accountHolder": "John Doe"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "userId": 1,
  "bankName": "Bank of America",
  "accountNumber": "123456789",
  "accountHolder": "John Doe",
  "balance": 0.00
}
```

**Errors:**
- 400: User already has an account
- 500: Failed to create account

---

### Update Account
**PUT** `/users/me/account`

Update current user's bank account details.

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body (all fields optional):**
```json
{
  "bankName": "Chase Bank",
  "accountNumber": "987654321",
  "accountHolder": "John Doe"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "userId": 1,
  "bankName": "Chase Bank",
  "accountNumber": "987654321",
  "accountHolder": "John Doe",
  "balance": 5000.00
}
```

**Errors:**
- 404: Account not found

---

## Payment Endpoints

### Get Current User Payment History
**GET** `/users/me/payments`

Get payment history for current user.

**Headers:**
```
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
  "paymentCount": 3,
  "totalPaid": 15000.00,
  "payments": [
    {
      "id": 1,
      "userId": 1,
      "amount": 5000.00,
      "paymentDate": "2024-01-15T10:30:00Z",
      "bankName": "Bank of America",
      "referenceNumber": "REF-001",
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": 2,
      "userId": 1,
      "amount": 5000.00,
      "paymentDate": "2024-01-20T14:45:00Z",
      "bankName": "Bank of America",
      "referenceNumber": "REF-002",
      "createdAt": "2024-01-20T14:45:00Z"
    },
    {
      "id": 3,
      "userId": 1,
      "amount": 5000.00,
      "paymentDate": "2024-01-25T09:15:00Z",
      "bankName": "Chase Bank",
      "referenceNumber": null,
      "createdAt": "2024-01-25T09:15:00Z"
    }
  ]
}
```

---

### Add Payment (Current User)
**POST** `/users/me/payments`

Add a payment for current user.

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "amount": 5000.00,
  "paymentDate": "2024-02-01T10:00:00Z",
  "bankName": "Bank of America",
  "referenceNumber": "REF-003"
}
```

**Response (201 Created):**
```json
{
  "id": 4,
  "userId": 1,
  "amount": 5000.00,
  "paymentDate": "2024-02-01T10:00:00Z",
  "bankName": "Bank of America",
  "referenceNumber": "REF-003",
  "createdAt": "2024-02-01T10:00:00Z"
}
```

---

### Get All Payments (Admin Only)
**GET** `/payments`

Get all payments across all users. **Admin only.**

**Headers:**
```
Authorization: Bearer <admin-token>
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "userId": 1,
    "amount": 5000.00,
    "paymentDate": "2024-01-15T10:30:00Z",
    "bankName": "Bank of America",
    "referenceNumber": "REF-001",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": 2,
    "userId": 2,
    "amount": 3000.00,
    "paymentDate": "2024-01-16T11:00:00Z",
    "bankName": "Chase Bank",
    "referenceNumber": "REF-002",
    "createdAt": "2024-01-16T11:00:00Z"
  }
]
```

---

### Add Payment for User (Admin Only)
**POST** `/payments/user/{userId}`

Add a payment for a specific user. **Admin only.**

**Parameters:**
- `userId` (path): Target user ID

**Headers:**
```
Authorization: Bearer <admin-token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "amount": 2500.00,
  "paymentDate": "2024-02-01T10:00:00Z",
  "bankName": "Wells Fargo",
  "referenceNumber": "REF-100"
}
```

**Response (201 Created):**
```json
{
  "id": 10,
  "userId": 2,
  "amount": 2500.00,
  "paymentDate": "2024-02-01T10:00:00Z",
  "bankName": "Wells Fargo",
  "referenceNumber": "REF-100",
  "createdAt": "2024-02-01T10:00:00Z"
}
```

---

### Update Payment (Admin Only)
**PUT** `/payments/{id}`

Update a payment. **Admin only.**

**Parameters:**
- `id` (path): Payment ID

**Headers:**
```
Authorization: Bearer <admin-token>
Content-Type: application/json
```

**Request Body (all fields optional):**
```json
{
  "amount": 5500.00,
  "paymentDate": "2024-02-02T10:00:00Z",
  "bankName": "Chase Bank",
  "referenceNumber": "REF-003-UPDATED"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "userId": 1,
  "amount": 5500.00,
  "paymentDate": "2024-02-02T10:00:00Z",
  "bankName": "Chase Bank",
  "referenceNumber": "REF-003-UPDATED",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

### Delete Payment (Admin Only)
**DELETE** `/payments/{id}`

Delete a payment. **Admin only.**

**Parameters:**
- `id` (path): Payment ID

**Headers:**
```
Authorization: Bearer <admin-token>
```

**Response (204 No Content)**

---

### Clear User Payments (Admin Only)
**POST** `/payments/user/{userId}/clear`

Delete all payments for a user (when cashed out). **Admin only.**

**Parameters:**
- `userId` (path): User ID

**Headers:**
```
Authorization: Bearer <admin-token>
```

**Response (204 No Content)**

**Errors:**
- 404: No payments found for user

---

## Status Codes

- **200** - OK (successful GET, PUT)
- **201** - Created (successful POST)
- **204** - No Content (successful DELETE)
- **400** - Bad Request (validation error)
- **401** - Unauthorized (missing/invalid token)
- **403** - Forbidden (insufficient permissions)
- **404** - Not Found (resource doesn't exist)
- **500** - Internal Server Error

---

## Error Examples

### Invalid Token
**Response (401):**
```json
{
  "error": "User not authenticated"
}
```

### Admin Only Endpoint
**Response (403):**
```json
{
  "error": "Forbidden"
}
```

### Duplicate Username
**Response (400):**
```json
{
  "error": "Username or phone number already exists"
}
```

---

## Rate Limiting

Currently not implemented. Add rate limiting middleware in production.

---

## CORS

Currently configured to allow all origins. In production, restrict to specific domains in `Program.cs`.

---

## Security Best Practices

1. Always use HTTPS in production
2. Keep JWT_SECRET secure and never commit it
3. Validate all input on backend
4. Use strong passwords (minimum 8 characters, mix of cases and numbers)
5. Implement rate limiting
6. Monitor for suspicious activity
7. Regularly update dependencies
