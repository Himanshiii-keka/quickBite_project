# 🚀 QuickBite Application - Exact URLs & Running Guide

## 📍 **Exact Swagger URL**

Based on your `launchSettings.json` configuration, the Swagger URL will be:

### **HTTPS Profile (Default):**
```
https://localhost:7112/swagger/index.html
```

### **HTTP Profile:**
```
http://localhost:5227/swagger/index.html
```

### **IIS Express:**
```
https://localhost:44363/swagger/index.html
```

---

## 🔧 **Run Application Using Solution File**

### **Method 1: Open Solution in Visual Studio**

1. **Open Visual Studio 2022+**
2. **File → Open → Project/Solution**
3. Navigate to:
   ```
   C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\
   ```
4. **Select:** `startup_project.slnx`
5. **Click Open**

### **Method 2: Run from Command Line (PowerShell)**

```powershell
# Navigate to solution directory
cd "C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\"

# Open with Visual Studio
start startup_project.slnx
```

---

## ▶️ **Running the Application**

### **Option A: Using Visual Studio IDE**

1. **Load the solution** (`startup_project.slnx`)
2. **Select launch profile** from the dropdown menu in the toolbar:
   - **https** (Recommended - Secure)
   - **http** (Development)
   - **IIS Express** (Alternative)
3. **Press F5** or click the **▶ Run** button
4. **Browser automatically opens** to Swagger UI

---

### **Option B: Using PowerShell / Command Line**

#### **Run with HTTPS Profile (Default):**
```powershell
cd "C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\"
dotnet run --launch-profile https
```

#### **Run with HTTP Profile:**
```powershell
dotnet run --launch-profile http
```

#### **Run with IIS Express:**
```powershell
dotnet run --launch-profile "IIS Express"
```

#### **Run with default profile:**
```powershell
dotnet run
```

---

### **Option C: Using Visual Studio Package Manager Console**

```powershell
# In Package Manager Console (Tools → NuGet Package Manager → Package Manager Console)
dotnet run
```

---

## 📊 **Launch Profiles Configuration**

Your `launchSettings.json` defines three profiles:

| Profile | Protocol | Port | Application URL | SSL Port |
|---------|----------|------|-----------------|----------|
| **https** | HTTPS + HTTP | 5227 | `https://localhost:7112` | 7112 |
| **http** | HTTP | 5227 | `http://localhost:5227` | N/A |
| **IIS Express** | HTTPS + HTTP | - | `https://localhost:44363` | 44363 |

---

## 📌 **Default Swagger URLs by Profile**

| Profile | Swagger URL | API Base URL |
|---------|-------------|--------------|
| **HTTPS** (Recommended) | `https://localhost:7112/swagger/index.html` | `https://localhost:7112/api/` |
| **HTTP** | `http://localhost:5227/swagger/index.html` | `http://localhost:5227/api/` |
| **IIS Express** | `https://localhost:44363/swagger/index.html` | `https://localhost:44363/api/` |

---

## 🔄 **Database Migrations Before Running**

**Important:** Apply migrations before first run:

### **From Package Manager Console:**
```powershell
Update-Database
```

### **From PowerShell:**
```powershell
cd "C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\"
dotnet ef database update
```

---

## ✅ **Verification Steps**

After running the application, verify it's working:

### **1. Check Console Output**
Look for messages like:
```
✅ Database connection successful.
📦 Distributed cache: in-memory (set Redis:ConnectionString or ConnectionStrings:Redis to use Redis).
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7112
      Now listening on: http://localhost:5227
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

### **2. Open Swagger UI**
Visit one of the Swagger URLs above in your browser.

### **3. Test API Endpoints**
Use Swagger UI to test endpoints like:
- `GET /api/restaurants` - Get all restaurants
- `POST /api/auth/login` - Login (get JWT token)

---

## 🛑 **Stop the Application**

- **Visual Studio:** Click the ⏹️ **Stop** button or press **Shift+F5**
- **Command Line:** Press **Ctrl+C** in the terminal

---

## 🔗 **Solution File Details**

```
File: startup_project.slnx
Location: C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\startup_project.slnx
Type: Visual Studio Solution File (slnx = Lightweight solution)
Project: startup_project.csproj
Framework: .NET 8
```

---

## 🐛 **Troubleshooting**

### **Port Already in Use**
If `localhost:7112` is in use, change in `launchSettings.json`:
```json
"applicationUrl": "https://localhost:7113;http://localhost:5228"
```

### **HTTPS Certificate Error**
```powershell
# Install development certificate
dotnet dev-certs https --trust
```

### **Database Connection Failed**
Ensure LocalDB is running:
```powershell
sqllocaldb start MSSQLLocalDB
```

### **Solution Won't Open**
Try opening the project file directly:
```powershell
cd "C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\"
dotnet build
```

---

## 📝 **Quick Start Summary**

### **Fastest Way to Run:**
```powershell
# 1. Navigate to project
cd "C:\Users\himanshi.m\source\repos\quickBite_project\onboarding_project\"

# 2. Apply migrations
dotnet ef database update

# 3. Run application
dotnet run --launch-profile https

# 4. Open browser
# https://localhost:7112/swagger/index.html
```

### **Using Visual Studio:**
1. Open `startup_project.slnx`
2. Press **F5**
3. Browser opens to Swagger automatically

---

## 📚 **Additional Commands**

```powershell
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Clean build
dotnet clean

# Run tests (if test projects exist)
dotnet test

# Watch mode (auto-reload on code changes)
dotnet watch run --launch-profile https

# Publish for production
dotnet publish -c Release
```

---

**Solution File:** `startup_project.slnx`  
**Default Swagger URL:** `https://localhost:7112/swagger/index.html`  
**Framework:** .NET 8  
**Last Updated:** 2026-01-13
