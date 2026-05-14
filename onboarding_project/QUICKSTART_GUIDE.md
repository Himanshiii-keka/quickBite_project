# QuickBite Project - Startup & Migration Guide

## 📋 Project Overview

**QuickBite** is an ASP.NET Core 8 API application for a food delivery system. The project includes:

- **Database**: SQL Server (LocalDB) with Entity Framework Core
- **Authentication**: JWT-based authentication
- **Caching**: Redis (optional) with in-memory fallback
- **API Documentation**: Swagger/OpenAPI

### Key Components:
- **Controllers**: Auth, Restaurants, Menu Items, Cart, Orders, Admin
- **Services**: AuthService, RestaurantService, MenuItemService, CartService, OrderService
- **Database Entities**: User, Restaurant, MenuItem, Order, OrderItem, Cart, CartItem
- **Migrations**: Initial setup and cart functionality

---

## 🚀 Getting Started

### Prerequisites
- **.NET 8 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server** with LocalDB (included with Visual Studio)
- **Visual Studio 2022+** or **VS Code** with C# extension
- **Git** (for version control)

### Project Location
```
C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\
```

---

## 📦 Project Structure

```
onboarding_project/
├── Program.cs                 # Application startup configuration
├── appsettings.json           # Configuration (DB connection, JWT, Redis)
├── Controllers/               # API endpoints
│   ├── AuthController.cs
│   ├── RestaurantsController.cs
│   ├── CartController.cs
│   ├── OrdersController.cs
│   ├── AdminController.cs
│   └── ...
├── Services/                  # Business logic
│   ├── AuthService.cs
│   ├── RestaurantService.cs
│   ├── CartService.cs
│   ├── OrderService.cs
│   └── ...
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
├── Models/                    # Data models
│   ├── User.cs
│   ├── Restaurant.cs
│   ├── MenuItem.cs
│   ├── Order.cs
│   ├── Cart.cs
│   └── ...
├── Migrations/                # Database migrations
│   ├── 20260512213054_InitialCreate.cs
│   └── 20260512221004_AddCart.cs
└── Common/                    # Utility classes
    ├── PublicReadCache.cs
    └── ClaimsPrincipalExtensions.cs
```

---

## 🗄️ Database Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QuickBiteDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Redis": {
    "ConnectionString": ""
  },
  "Jwt": {
    "Key": "QuickBite_SuperSecret_Key_ChangeThis_InProduction_Min32Chars!",
    "Issuer": "QuickBiteApp",
    "Audience": "QuickBiteUsers",
    "ExpiryMinutes": 60
  }
}
```

---

## 🛠️ Setup & Startup Commands

### Option 1: Using Visual Studio IDE

#### Step 1: Open the Project
1. Open Visual Studio 2022+
2. File → Open → Project/Solution
3. Navigate to: `C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\`
4. Select `startup_project.csproj`

#### Step 2: Restore NuGet Packages
- Right-click project → "Restore NuGet Packages"
- Or: `Tools → NuGet Package Manager → Package Manager Console`

#### Step 3: Apply Migrations
Open **Package Manager Console** and run:
```powershell
Update-Database
```

#### Step 4: Run the Application
- Press **F5** or click the **Run** button
- The app will start at: `https://localhost:5001` (or similar)

---

### Option 2: Using PowerShell/Command Line

#### Step 1: Navigate to Project Directory
```powershell
cd C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\
```

#### Step 2: Restore NuGet Packages
```powershell
dotnet restore
```

#### Step 3: Apply Migrations to Database
```powershell
dotnet ef database update
```

#### Step 4: Run the Application
```powershell
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

---

### Option 3: Using Docker (Optional)

If you want to containerize the app:

#### Build Docker Image
```powershell
docker build -t quickbite:latest .
```

#### Run Docker Container
```powershell
docker run -p 5001:8080 quickbite:latest
```

---

## 🔄 Database Migration Commands

### View Current Migrations
```powershell
dotnet ef migrations list
```

**Expected Output:**
```
20260512213054_InitialCreate
20260512221004_AddCart
```

### Apply All Pending Migrations
```powershell
dotnet ef database update
```

### Apply Migration to Specific Version
```powershell
dotnet ef database update 20260512213054_InitialCreate
```

### Revert Last Migration
```powershell
dotnet ef database update 20260512213054_InitialCreate
```

### Create New Migration (After Model Changes)
```powershell
dotnet ef migrations add MigrationName
```

Example:
```powershell
dotnet ef migrations add AddUserPreferences
```

### Generate SQL Script (Preview Changes)
```powershell
dotnet ef migrations script --output migration.sql
```

### Generate SQL for Specific Migration Range
```powershell
dotnet ef migrations script 20260512213054_InitialCreate 20260512221004_AddCart --output migrations.sql
```

### Remove Last Migration (Before Applying)
```powershell
dotnet ef migrations remove
```

---

## 📡 API Endpoints

Once running, access the API at:

### Swagger UI (Interactive Documentation)
```
https://localhost:5001/swagger/index.html
```

### API Base URL
```
https://localhost:5001/api/
```

### Available Endpoints:

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user (returns JWT token)

#### Restaurants
- `GET /api/restaurants` - Get all restaurants
- `GET /api/restaurants/{id}` - Get restaurant details
- `GET /api/restaurants/{id}/menu` - Get restaurant menu

#### Menu Items
- `GET /api/menuitems` - Get all menu items
- `GET /api/menuitems/{id}` - Get menu item details

#### Cart
- `POST /api/cart/add` - Add item to cart
- `GET /api/cart` - Get user's cart
- `DELETE /api/cart/{itemId}` - Remove item from cart

#### Orders
- `POST /api/orders` - Create new order
- `GET /api/orders` - Get user's orders
- `GET /api/orders/{id}` - Get order details

#### Admin
- `POST /api/admin/restaurants` - Create restaurant (admin only)
- `POST /api/admin/menuitems` - Create menu item (admin only)

---

## 🔐 JWT Authentication

To use protected endpoints:

1. **Register/Login** to get JWT token:
```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

2. **Response includes JWT token**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { ... }
}
```

3. **Use token in requests**:
```bash
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

4. **In Swagger UI**:
   - Click the "Authorize" button
   - Paste your JWT token (without "Bearer " prefix)
   - Click "Authorize"

---

## 📊 Database Schema

### Tables Created by Migrations:

**InitialCreate (20260512213054):**
- Users
- Restaurants
- MenuItems
- Orders
- OrderItems

**AddCart (20260512221004):**
- Carts
- CartItems

### Key Relationships:
- User → Orders (1:Many)
- Restaurant → MenuItems (1:Many)
- Order → OrderItems (1:Many)
- MenuItem → OrderItems (Many:1)
- User → Cart (1:1)
- Cart → CartItems (1:Many)

---

## 🔍 Troubleshooting

### Issue: "No migrations detected"
```powershell
# Ensure you're in project directory
cd .\onboarding_project\

# Try again
dotnet ef migrations list
```

### Issue: "Database connection fails"
- Verify SQL Server is running: `sqllocaldb info mssqllocaldb`
- Check `appsettings.json` connection string
- Ensure LocalDB instance exists: `sqllocaldb create MSSQLLocalDB`

### Issue: "Cannot update database - migrations not applied"
```powershell
# Clear and reapply migrations
dotnet ef database drop --force
dotnet ef database update
```

### Issue: "Port 5001 already in use"
```powershell
# Run on different port
dotnet run --urls "https://localhost:5002"
```

### Issue: EF Core tools not installed
```powershell
dotnet tool install --global dotnet-ef
```

---

## 📝 Useful Dotnet Commands

```powershell
# Build solution
dotnet build

# Run tests (if test projects exist)
dotnet test

# Clean build
dotnet clean

# Publish application
dotnet publish -c Release

# Check solution info
dotnet sln list

# Watch for changes and rerun
dotnet watch run

# View project info
dotnet project-info
```

---

## 🎯 Quick Start Checklist

- [ ] Clone repository: `git clone https://github.com/Himanshiii-keka/quickBite_project`
- [ ] Navigate to project: `cd onboarding_project`
- [ ] Restore packages: `dotnet restore`
- [ ] Apply migrations: `dotnet ef database update`
- [ ] Run application: `dotnet run`
- [ ] Open Swagger: `https://localhost:5001/swagger/index.html`
- [ ] Test login endpoint
- [ ] Explore API using Swagger UI

---

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication in .NET](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- [Swagger/OpenAPI](https://swagger.io/)

---

## 🔗 Repository

**Repository URL:** https://github.com/Himanshiii-keka/quickBite_project  
**Branch:** main  
**Clone Command:**
```bash
git clone https://github.com/Himanshiii-keka/quickBite_project.git
```

---

**Last Updated:** 2025-01-09  
**Version:** .NET 8 | EF Core 8
