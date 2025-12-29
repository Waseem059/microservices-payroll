// docker-compose.yml
version: '3.8'

services:
  api-gateway:
    build:
      context: .
      dockerfile: ApiGateway/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - auth-service
      - payroll-service
      - integration-service
      - reporting-service

  auth-service:
    build:
      context: .
      dockerfile: AuthService/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  payroll-service:
    build:
      context: .
      dockerfile: PayrollService/Dockerfile
    ports:
      - "5002:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./PayrollService/payroll.db:/app/payroll.db

  integration-service:
    build:
      context: .
      dockerfile: IntegrationService/Dockerfile
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

  reporting-service:
    build:
      context: .
      dockerfile: ReportingService/Dockerfile
    ports:
      - "5004:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development

---

// .gitignore
bin/
obj/
.vs/
.vscode/
*.user
*.dll
*.exe
*.o
*.so
*.dylib
*.db
.DS_Store
appsettings.json
node_modules/

---

// README.md
# Payroll System Microservices

A demonstration of microservices architecture using .NET Core with 4 independent services communicating via REST APIs.

## Architecture Overview

```
┌─────────────────┐
│  API Gateway    │ (Port 5000)
└────────┬────────┘
         │
    ┌────┼────┬────────┬──────────┐
    │    │    │        │          │
┌───▼──┐ │ ┌──▼──┐ ┌───▼──┐ ┌──▼──┐
│Auth  │ │ │Pay  │ │Integ │ │Report
│      │ │ │roll │ │ration│ │ing
└──────┘ │ └─────┘ └──────┘ └─────┘
         │
      ports 5001-5004
```

## Services

### 1. Auth Service (Port 5001)
- User authentication
- JWT token generation and validation
- **Endpoints:**
  - `POST /api/auth/login` - Login with credentials
  - `POST /api/auth/validate` - Validate JWT token

### 2. Payroll Service (Port 5002)
- Employee data management
- Salary calculations
- Payroll processing
- **Endpoints:**
  - `GET /api/payroll/employees` - Get all employees
  - `POST /api/payroll/calculate` - Calculate salary
  - `POST /api/payroll/process` - Process payroll

### 3. Integration Service (Port 5003)
- Third-party integrations (Gusto mock)
- Sync payroll data
- **Endpoints:**
  - `POST /api/integration/sync-gusto` - Sync with Gusto
  - `GET /api/integration/gusto-status/{companyId}` - Get sync status

### 4. Reporting Service (Port 5004)
- Generate payroll reports
- Employee reports
- Payroll summaries
- **Endpoints:**
  - `POST /api/report/payroll-summary` - Generate monthly summary
  - `GET /api/report/employee-report/{employeeId}` - Employee report

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker (optional)
- Git

### Local Setup (Without Docker)

1. Clone repository
2. Navigate to project folder
3. Build solution:
   ```
   dotnet build
   ```

4. Run each service in separate terminal:
   ```
   # Terminal 1
   cd AuthService && dotnet run

   # Terminal 2
   cd PayrollService && dotnet run

   # Terminal 3
   cd IntegrationService && dotnet run

   # Terminal 4
   cd ReportingService && dotnet run

   # Terminal 5
   cd ApiGateway && dotnet run
   ```

5. Visit http://localhost:5000/swagger for API documentation

### Docker Setup

```
docker-compose up --build
```

Services will be available at:
- API Gateway: http://localhost:5000
- Auth Service: http://localhost:5001
- Payroll Service: http://localhost:5002
- Integration Service: http://localhost:5003
- Reporting Service: http://localhost:5004

## Sample API Calls

### 1. Login
```
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password123"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "message": "Login successful"
}
```

### 2. Get Employees
```
curl http://localhost:5000/api/payroll/employees
```

### 3. Calculate Payroll
```
curl -X POST http://localhost:5000/api/payroll/calculate \
  -H "Content-Type: application/json" \
  -d '{"employeeId":1,"baseSalary":50000}'
```

### 4. Generate Report
```
curl -X POST http://localhost:5000/api/report/payroll-summary \
  -H "Content-Type: application/json" \
  -d '{"month":12,"year":2024}'
```

## Architecture Decisions

### 1. Service Decomposition (Single Responsibility)
- Each service handles one domain (Auth, Payroll, Integration, Reporting)
- Services can scale independently
- Technology can be different per service if needed

### 2. API Gateway Pattern
- Single entry point for clients
- Routes requests to appropriate services
- Simplifies client implementation

### 3. REST Communication
- Simple HTTP/REST between services
- Easy to understand and debug
- Can be upgraded to gRPC later

### 4. Database Per Service
- Each service has own SQLite database
- Independent data storage
- Prevents coupling through shared DB

### 5. No Message Queue (For Now)
- Direct HTTP calls for simplicity
- Can be upgraded to RabbitMQ/Kafka later
- Good for learning microservices basics

## Future Improvements

1. Add message queues (RabbitMQ, Kafka)
2. Implement service discovery
3. Add logging (Serilog, ELK)
4. Implement circuit breaker pattern
5. Add distributed tracing
6. Kubernetes deployment

## Technology Stack

- **Framework:** .NET 8
- **API:** ASP.NET Core Web API
- **Database:** SQLite (can upgrade to PostgreSQL)
- **Authentication:** JWT
- **Containerization:** Docker
- **Orchestration:** Docker Compose (can upgrade to Kubernetes)

## Developer Notes

- All services run independently
- Services communicate via HTTP
- Authentication is mocked (for demo)
- Database is in-memory for simplicity
- No external dependencies needed

## Contributing

This is a learning project. Feel free to:
- Add more features
- Improve error handling
- Add unit tests
- Implement new patterns

Happy learning! 🚀
