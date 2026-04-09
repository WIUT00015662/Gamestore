# Gamestore

A modern, full-stack e-commerce platform for purchasing and discovering games with real-time price tracking and automated notifications.

## 🎮 Features

### Core Platform
- **Game Discovery & Metadata**: Advanced search, filtering by genre/platform, pagination, and sorting
- **Real-Time Price Analysis**: Automated polling system that identifies top 5 discounted games daily
- **Smart Notifications**: Email alerts when prices drop >20% (disabled in simulation mode)
- **Shopping Cart & Checkout**: Full cart management with multiple payment simulation options (Visa, Bank Transfer, IBox)
- **Order Management**: Complete order tracking with status updates

### Community
- **Hierarchical Comments**: Users can reply to, quote, and reply-to-quote other comments
- **Moderation System**: Moderators can ban users from commenting (temporary or permanent)
- **Role-Based Access**: Admin, Manager, Moderator, and regular user roles with granular permissions

### Technical Features
- **JWT Authentication**: Secure token-based authentication with refresh token support
- **CORS Protection**: Properly configured cross-origin policies
- **Global Exception Handling**: Comprehensive error handling with detailed logging
- **Request/Response Logging**: Full request-response pipeline logging for debugging
- **Unit Tests**: 127+ unit tests covering 50%+ of critical business logic

## 🏗️ Architecture

### Modular Monolith with Clean Architecture

```
Gamestore.Api (REST API Layer)
├── Controllers (HTTP endpoints)
├── Middleware (logging, exception handling)
├── Configuration (DI, settings)
└── Services (API business logic)

Gamestore.BLL (Business Logic Layer)
├── Services (domain-specific business rules)
├── DTOs (data transfer objects)
└── Validators (business validation)

Gamestore.DAL (Data Access Layer)
├── Data (DbContext, migrations)
├── Repositories (data access patterns)
├── Configurations (EF Core mappings)
└── Migrations (schema versioning)

Gamestore.Domain (Domain Layer)
├── Entities (core domain models)
├── Repositories (abstract interfaces)
├── Exceptions (custom domain exceptions)
└── Value Objects (game details, pricing, etc.)

Gamestore.ReactUI (Frontend)
├── Pages (React components)
├── Components (reusable UI)
├── API (REST client)
└── Auth (authentication flow)
```

## 🚀 Tech Stack

### Backend
- **.NET 8** with C# 12
- **Entity Framework Core** (ORM)
- **SQL Server** (database)
- **JWT** (authentication)
- **xUnit** (testing)
- **Serilog** (structured logging)

### Frontend
- **React 18** with TypeScript
- **Vite** (build tool)
- **React Router** (routing)
- **Axios** (HTTP client)
- **TailwindCSS** (styling)

### DevOps
- **Docker** (containerization)
- **GitHub Actions** (CI/CD)
- **Google Cloud Platform** (hosting)
- **Nginx** (reverse proxy)
- **Certbot** (HTTPS/SSL)

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server 2019+
- Node.js 20+
- Docker & Docker Compose (for containerized deployment)
- Git

## 🛠️ Local Setup

### 1. Clone and Install

```bash
git clone https://github.com/WIUT00015662/Gamestore.git
cd Gamestore
```

### 2. Configure Environment

```bash
# Create local .env file
cp .env.example .env

# Edit .env with your local settings
```

**Key environment variables:**
```
ConnectionStrings__DefaultConnection=Server=(localdb)\mssqllocaldb;Database=GamestoreDb;Trusted_Connection=True;
JWT_KEY=your-secret-key-min-32-chars
SMTP_HOST=your-email-service
```

### 3. Database Setup

```bash
cd Gamestore
dotnet restore
dotnet ef database update --project Gamestore.DAL --startup-project Gamestore.Api
```

### 4. Run Backend

```bash
cd Gamestore/Gamestore.Api
dotnet run
# API will be available at http://localhost:5000
```

### 5. Run Frontend

```bash
cd Gamestore/Gamestore.ReactUI
npm install
npm run dev
# Frontend will be available at http://localhost:5173
```

## 📦 Docker Deployment

### Development Environment

```bash
docker-compose up -d
# API: http://localhost:8080
# Frontend: http://localhost:3000
# Database: localhost:1433
```

### Production Environment

```bash
docker-compose -f docker-compose.prod.yml up -d
# Configure deployment/gcp/.env.prod with production values
```

## 🧪 Running Tests

```bash
# Run all unit tests
dotnet test Gamestore/Gamestore.UnitTests/Gamestore.UnitTests.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 📡 API Endpoints

### Games
```
GET    /api/games              - List all games (paginated)
GET    /api/games/{id}         - Get game details
POST   /api/games              - Create game (admin)
PUT    /api/games/{id}         - Update game (manager)
DELETE /api/games/{id}         - Soft delete game (admin)
```

### Deals & Discounts
```
GET    /api/deals/discounts    - Get top 5 current discounts
POST   /api/deals/poll         - Trigger discount polling (manual)
```

### Comments
```
GET    /api/games/{id}/comments   - Get game comments
POST   /api/games/{id}/comments   - Add comment
PUT    /api/comments/{id}         - Edit own comment
DELETE /api/comments/{id}         - Delete own comment
```

### Orders
```
GET    /api/orders                - Get user orders
POST   /api/orders                - Create order
GET    /api/orders/{id}           - Get order details
```

### Authentication
```
POST   /api/auth/register         - Register user
POST   /api/auth/login            - Login user
POST   /api/auth/refresh          - Refresh JWT token
```

## 🔐 Security

- **JWT Tokens**: 2-hour expiry with refresh tokens
- **Password Hashing**: PBKDF2 with salt
- **CORS**: Whitelist-based origin validation
- **Input Validation**: Server-side validation on all endpoints
- **SQL Injection Protection**: Parameterized queries via EF Core
- **Rate Limiting**: Implemented via middleware
- **HTTPS**: Enforced in production (via Certbot)

## 📊 Database Schema

### Core Tables
- `Games` - Game catalog with metadata
- `Genres` - Game genres (hierarchical)
- `Platforms` - Gaming platforms (mobile, web, desktop, console)
- `Publishers` - Game publishers
- `Orders` - User orders and transactions
- `OrderGames` - Order line items
- `Comments` - User comments with threading support
- `CommentBans` - User banning records
- `Users` - User accounts
- `Roles` - Permission roles
- `RolePermissions` - Role-permission mappings
- `GameVendorOffers` - Real-time vendor prices
- `GameDiscountSnapshots` - Historical discount records

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

1. **Code Quality** (on every push)
   - C# linting with StyleCop
   - TypeScript linting with ESLint
   - Code compilation

2. **Testing** (on every push)
   - Unit test execution
   - Code coverage validation (min 50%)

3. **Build** (on main push)
   - Docker image build for API
   - Docker image build for Frontend
   - Push to Docker Hub

4. **Deploy** (on main push)
   - Pull latest code on GCP VM
   - Stop old containers
   - Start new containers with updated images
   - Run database migrations
   - Health check verification

## 🚢 Production Deployment

### GCP VM Setup

```bash
# 1. On VM - Install prerequisites
sudo apt-get update
sudo apt-get install -y docker.io docker-compose git

# 2. Clone repository
git clone https://github.com/WIUT00015662/Gamestore.git /opt/gamestore
cd /opt/gamestore

# 3. Configure production environment
cp deployment/gcp/.env.prod.example deployment/gcp/.env.prod
# Edit with production secrets and values

# 4. Deploy
chmod +x deployment/gcp/deploy.sh
./deployment/gcp/deploy.sh

# 5. Setup HTTPS
chmod +x deployment/gcp/setup-https-certbot.sh
./deployment/gcp/setup-https-certbot.sh yourdomain.com api.yourdomain.com your-email@example.com
```

## 📝 Logging

### Backend Logs
```
Gamestore/Gamestore.Api/Logs/
├── error-*.log        (errors and exceptions)
├── information-*.log  (general info)
└── debug-*.log        (debug traces)
```

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Check connection string in .env
# Verify SQL Server is running
# Run migrations manually
dotnet ef database update --project Gamestore.DAL --startup-project Gamestore.Api
```

### Docker Build Failures
```bash
# Clean up Docker resources
docker system prune -a
docker-compose down -v

# Rebuild with no cache
docker-compose build --no-cache
```

### Authentication Failures
```bash
# Verify JWT_KEY is set in .env
# Check token expiration (default 2 hours)
# Clear browser cookies and retry login
```

## 📚 Additional Resources

- **Deployment Guide**: See `deployment/gcp/README.md`
- **Database Docs**: See `deployment/gcp/README.md`
- **GitHub Repository**: https://github.com/WIUT00015662/Gamestore

## 👤 Author

WIUT00015662 - Gamestore Development Project

---

**Last Updated**: April 10, 2026  
**Status**: Production Ready ✅
