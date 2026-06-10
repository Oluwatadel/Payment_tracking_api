# Swagger Quick Reference Card

## Access Swagger UI
```
http://localhost:5000/swagger
```

## Login Flow
```
1. POST /api/auth/login
   Request: { "username": "...", "password": "..." }
   Response: { "token": "...", "user": {...} }

2. Click "Authorize" button
   Paste: Bearer <token>

3. All endpoints now require authentication
```

## Key Endpoints

### Authentication
```
POST   /api/auth/login           Login
GET    /api/auth/profile         Get profile
```

### Users
```
GET    /api/users/me             Current user profile
GET    /api/users/{id}           User details (admin)
GET    /api/users                List all users (admin)
POST   /api/users                Create user (admin)
PUT    /api/users/{id}           Update user (admin)
DELETE /api/users/{id}           Delete user (admin)
GET    /api/users/{id}/account   User's account
PUT    /api/users/{id}/account   Update account (admin)
GET    /api/users/{id}/payments  User's payments
```

### Payments
```
GET    /api/payments             All/own payments
POST   /api/payments             Create payment (admin)
GET    /api/payments/{id}        Payment details
PUT    /api/payments/{id}        Update payment (admin)
DELETE /api/payments/{id}        Delete payment (admin)
POST   /api/payments/{id}/clear  Clear balance (admin)
```

## HTTP Status Codes
```
200  OK              - Request successful
201  Created         - Resource created
400  Bad Request     - Invalid request
401  Unauthorized    - Missing/invalid token
403  Forbidden       - Insufficient permissions
404  Not Found       - Resource not found
500  Server Error    - Unexpected error
```

## Request Headers
```
Authorization: Bearer <token>
Content-Type: application/json
```

## Common Errors & Fixes

| Error | Fix |
|-------|-----|
| 401 Unauthorized | Click "Authorize", paste token |
| 403 Forbidden | Check if you have Admin role |
| 404 Not Found | Verify resource exists |
| Swagger page blank | Run in Development mode |

## Using Try-it-out

1. Click on any endpoint
2. Click "Try it out"
3. Fill in parameters
4. Click "Execute"
5. View response

## Export OpenAPI Spec

```bash
curl http://localhost:5000/swagger/v1/swagger.json > swagger.json
```

Use in Postman:
1. Import → Link
2. Paste: `http://localhost:5000/swagger/v1/swagger.json`
3. Continue → Import

## Documentation Files

| File | Purpose |
|------|---------|
| SWAGGER_GUIDE.md | Complete user guide |
| API_DOCUMENTATION.md | Detailed endpoint documentation |
| SWAGGER_SETUP_CHECKLIST.md | Setup and configuration guide |
| SWAGGER_QUICK_REFERENCE.md | This quick reference |

## Example cURL Requests

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Get current user
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer <token>"

# Create payment (admin)
curl -X POST http://localhost:5000/api/payments \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"amount":500,"bankName":"ABC Bank"}'
```

## Tips & Tricks

✓ **Authorize once:** Token persists for entire session  
✓ **Try-it-out:** Best way to test endpoints  
✓ **Check status codes:** Indicates success/failure  
✓ **Use examples:** Copy curl from Swagger  
✓ **Export JSON:** For integration with other tools  

---

**For detailed information, see SWAGGER_GUIDE.md**
