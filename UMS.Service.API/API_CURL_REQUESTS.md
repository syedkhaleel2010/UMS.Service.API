# UMS Service API - cURL Request Examples

## Base URLs
- **HTTP**: `http://localhost:5009`
- **HTTPS**: `https://localhost:7227`

## Authentication
Most endpoints require JWT Bearer token authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your_jwt_token>
```

## Localization
To get responses in different languages, use:
- Query parameter: `?culture=ar` or `?culture=en`
- Header: `language: ar` or `language: en`

---

## 1. Authentication Endpoints

### 1.1 Login (Anonymous)
**POST** `/api/Auth/login`

Get JWT token by providing username and password.

```bash
curl -X POST "http://localhost:5009/api/Auth/login" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/Auth/login?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiration": "2024-01-01T12:00:00Z",
  "username": "admin",
  "email": "admin@example.com",
  "roles": ["Admin"]
}
```

---

### 1.2 Register User (Admin Only)
**POST** `/api/Auth/register`

Create a new user. Requires Admin role.

```bash
curl -X POST "http://localhost:5009/api/Auth/register" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true
  }'
```

**Minimal request (ID is optional):**
```bash
curl -X POST "http://localhost:5009/api/Auth/register" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "Password123!"
  }'
```

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/Auth/register?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "Password123!",
    "firstName": "محمد",
    "lastName": "أحمد"
  }'
```

---

### 1.3 Update User Details (Admin Only)
**PUT** `/api/Auth/update-userdetails`

Update existing user information. Requires Admin role.

```bash
curl -X PUT "http://localhost:5009/api/Auth/update-userdetails" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "user-guid-here",
    "username": "updateduser",
    "email": "updated@example.com",
    "password": "NewPassword123!",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true
  }'
```

**With Arabic localization:**
```bash
curl -X PUT "http://localhost:5009/api/Auth/update-userdetails?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "user-guid-here",
    "username": "updateduser",
    "email": "updated@example.com",
    "password": "NewPassword123!",
    "firstName": "فاطمة",
    "lastName": "علي"
  }'
```

---

### 1.4 Delete User (Admin Only)
**DELETE** `/api/Auth/deleteUser`

Delete a user. Requires Admin role.

```bash
curl -X DELETE "http://localhost:5009/api/Auth/deleteUser" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "user-guid-here",
    "username": "usertodelete"
  }'
```

**With Arabic localization:**
```bash
curl -X DELETE "http://localhost:5009/api/Auth/deleteUser?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "user-guid-here",
    "username": "usertodelete"
  }'
```

---

### 1.5 Reset User Password (Authorized)
**POST** `/api/Auth/UserPasswordResetRequest`

Reset password for the authenticated user.

```bash
curl -X POST "http://localhost:5009/api/Auth/UserPasswordResetRequest" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "oldPassword": "OldPassword123!",
    "password": "NewPassword123!",
    "confirmPassword": "NewPassword123!",
    "username": "currentuser",
    "isAdmin": false
  }'
```

**Note:** Password must be 8-15 characters, contain at least 1 number, 1 special character, and mixture of uppercase and lowercase letters.

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/Auth/UserPasswordResetRequest?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "oldPassword": "OldPassword123!",
    "password": "NewPassword123!",
    "confirmPassword": "NewPassword123!",
    "username": "currentuser",
    "isAdmin": false
  }'
```

---

### 1.6 Refresh Token (Anonymous)
**POST** `/api/Auth/refresh`

Refresh an existing JWT token.

```bash
curl -X POST "http://localhost:5009/api/Auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "your_existing_jwt_token"
  }'
```

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/Auth/refresh?culture=ar" \
  -H "Content-Type: application/json" \
  -d '{
    "token": "your_existing_jwt_token"
  }'
```

---

### 1.7 Get Current User (Authorized)
**GET** `/api/Auth/me`

Get information about the currently authenticated user.

```bash
curl -X GET "http://localhost:5009/api/Auth/me" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**With Arabic localization:**
```bash
curl -X GET "http://localhost:5009/api/Auth/me?culture=ar" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
{
  "userId": "user-guid-here",
  "username": "currentuser",
  "email": "user@example.com"
}
```

---

## 2. User Role Management Endpoints

### 2.1 Create Role (Admin Only)
**POST** `/api/UserRole/CreateRole`

Create a new role. Requires Admin role.

```bash
curl -X POST "http://localhost:5009/api/UserRole/CreateRole" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "",
    "roleName": "Manager",
    "description": "Manager role with limited permissions"
  }'
```

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/UserRole/CreateRole?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "",
    "roleName": "مدير",
    "description": "دور المدير مع صلاحيات محدودة"
  }'
```

---

### 2.2 Assign Role to User (Admin Only)
**POST** `/api/UserRole/AssignRole`

Assign a role to a user. Requires Admin role.

```bash
curl -X POST "http://localhost:5009/api/UserRole/AssignRole" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "",
    "userId": "user-guid-here",
    "userName": "username",
    "roleId": "role-guid-here"
  }'
```

**With Arabic localization:**
```bash
curl -X POST "http://localhost:5009/api/UserRole/AssignRole?culture=ar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "id": "",
    "userId": "user-guid-here",
    "userName": "username",
    "roleId": "role-guid-here"
  }'
```

---

## 3. Test Endpoints

### 3.1 Welcome Message
**GET** `/api/Test/welcome`

Get a welcome message (localization test endpoint).

```bash
curl -X GET "http://localhost:5009/api/Test/welcome"
```

**English:**
```bash
curl -X GET "http://localhost:5009/api/Test/welcome?culture=en"
```

**Arabic:**
```bash
curl -X GET "http://localhost:5009/api/Test/welcome?culture=ar"
```

**Using header for localization:**
```bash
curl -X GET "http://localhost:5009/api/Test/welcome" \
  -H "language: ar"
```

**Response (English):**
```json
{
  "message": "Welcome"
}
```

**Response (Arabic):**
```json
{
  "message": "مرحباً"
}
```

---

## Complete Workflow Example

### Step 1: Login to get token
```bash
TOKEN=$(curl -s -X POST "http://localhost:5009/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }' | jq -r '.token')

echo "Token: $TOKEN"
```

### Step 2: Use token for authenticated requests
```bash
curl -X GET "http://localhost:5009/api/Auth/me" \
  -H "Authorization: Bearer $TOKEN"
```

### Step 3: Create a role (Admin only)
```bash
curl -X POST "http://localhost:5009/api/UserRole/CreateRole" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "roleName": "Editor",
    "description": "Editor role"
  }'
```

### Step 4: Register a new user (Admin only)
```bash
curl -X POST "http://localhost:5009/api/Auth/register" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "username": "editor1",
    "email": "editor1@example.com",
    "password": "Editor123!",
    "firstName": "Editor",
    "lastName": "User"
  }'
```

---

## Error Response Format

All error responses follow this format:

```json
{
  "success": false,
  "msg": "Error message here",
  "data": null,
  "errorlst": [
    {
      "error": "Detailed error message",
      "value": "Value that caused error"
    }
  ]
}
```

## Success Response Format

All success responses follow this format:

```json
{
  "success": true,
  "msg": "Success message here",
  "data": {
    // Response data
  }
}
```

---

## Notes

1. **JWT Token**: Save the token from login response and use it in subsequent requests
2. **Token Expiration**: Tokens expire after 60 minutes (configurable in JwtSettings)
3. **Localization**: Use `?culture=ar` or `?culture=en` query parameter, or `language` header
4. **HTTPS**: Replace `http://localhost:5009` with `https://localhost:7227` for HTTPS
5. **Admin Role**: Some endpoints require Admin role - ensure your user has this role
6. **Password Requirements**: 
   - Minimum 6 characters for registration
   - 8-15 characters for password reset
   - Must contain uppercase, lowercase, number, and special character for password reset

---

## Quick Reference

| Endpoint | Method | Auth Required | Role Required |
|----------|--------|---------------|---------------|
| `/api/Auth/login` | POST | No | - |
| `/api/Auth/register` | POST | Yes | Admin |
| `/api/Auth/update-userdetails` | PUT | Yes | Admin |
| `/api/Auth/deleteUser` | DELETE | Yes | Admin |
| `/api/Auth/UserPasswordResetRequest` | POST | Yes | - |
| `/api/Auth/refresh` | POST | No | - |
| `/api/Auth/me` | GET | Yes | - |
| `/api/UserRole/CreateRole` | POST | Yes | Admin |
| `/api/UserRole/AssignRole` | POST | Yes | Admin |
| `/api/Test/welcome` | GET | No | - |

