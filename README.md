# LoanTrack API

A RESTful backend API for managing loan and credit applications, built with ASP.NET Core 9, Entity Framework Core 9, and SQL Server. Developed as a portfolio project demonstrating corporate backend development practices.

## Tech Stack

- **Framework:** ASP.NET Core 9
- **ORM:** Entity Framework Core 9 (Code First)
- **Database:** SQL Server / LocalDB
- **Authentication:** ASP.NET Core Identity + JWT Bearer tokens with refresh token rotation
- **Documentation:** Swagger / OpenAPI
- **Logging:** Serilog (console + rolling file)

## Features

- User registration and login with role-based access control (Applicant, LoanOfficer, Admin)
- JWT authentication with refresh token rotation
- Loan application lifecycle: Submitted → UnderReview → Approved/Rejected → Disbursed → Repaid
- Automatic repayment schedule generation using the standard amortisation formula
- Reporting endpoints: paginated loan history, overdue loans, monthly disbursements, running balance
- Global exception handling middleware returning RFC 7807 Problem Details
- Layered architecture: API / Core / Infrastructure separation

## Project Structure
LoanTrack/

├── LoanTrack.API/            # HTTP layer — controllers, middleware, Program.cs

├── LoanTrack.Core/           # Business layer — entities, interfaces, DTOs

└── LoanTrack.Infrastructure/ # Data layer — EF Core, DbContext, migrations, services
## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server Express or LocalDB

### Setup

1. Clone the repository
https://github.com/Tsholofelo2607/LoanTrack

2. Apply database migrations
dotnet ef database update --project LoanTrack.Infrastructure --startup-project LoanTrack.API

3. Run the API
dotnet run --project LoanTrack.API --launch-profile http


4. Open Swagger UI in your browser
http://localhost:5019/swagger

## API Endpoints

### Auth
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | /api/auth/register | Public | Register a new user |
| POST | /api/auth/login | Public | Login and receive JWT |
| POST | /api/auth/refresh | Public | Refresh an expired JWT |

### Loan Products
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | /api/loanproducts | Authenticated | List all active loan products |

### Loan Applications
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| POST | /api/loanapplications | Applicant | Submit a loan application |
| GET | /api/loanapplications | Applicant/Officer/Admin | List applications |
| GET | /api/loanapplications/{id} | Owner/Officer/Admin | Get application by ID |
| PUT | /api/loanapplications/{id}/review | LoanOfficer/Admin | Approve or reject application |

### Reports
| Method | Endpoint | Access | Description |
|--------|----------|--------|-------------|
| GET | /api/reports/my-loans | Applicant | Paginated personal loan history |
| GET | /api/reports/overdue-loans | LoanOfficer/Admin | All loans with overdue installments |
| GET | /api/reports/monthly-disbursements | Admin | Total loans approved per month |
| GET | /api/reports/repayment-schedule/{id} | Owner/Officer/Admin | Installments with running balance |

## Authentication

All protected endpoints require a Bearer token in the Authorization header:

Authorization: Bearer {your_access_token}
Use the Swagger UI Authorize button to test endpoints interactively.