# SmartStock – Inventory Management System

A full-stack Inventory Management System built using **ASP.NET Core Web API, .NET 10, Angular, Entity Framework Core, PostgreSQL, and SignalR**.

SmartStock helps businesses manage products, inventory, stock movements, customers, customer orders, reports, and real-time cryptocurrency market prices through a modern, responsive web application.

## 🚀 Project Overview

SmartStock is designed with a layered architecture to provide maintainable, scalable, and reusable application code.

The project contains:
- **Backend:** ASP.NET Core Web API (.NET 10)
- **Frontend:** Angular
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Real-time communication:** SignalR
- **External market data:** Binance WebSocket API

## ✨ Features

### 1. Authentication
- User registration and login
- JWT-based authentication
- Protected routes
- HTTP authentication interceptor

### 2. Dashboard
- Inventory overview
- Product statistics
- Stock summary
- Recent activities

### 3. Product Management
- Add new products
- View product details
- Update product information
- Delete products
- SKU and category management
- Reorder level configuration

### 4. Inventory Management
- Manage available inventory
- Track stock quantities
- Monitor reorder levels
- View inventory information

### 5. Stock Movements
- Track stock movements
- Record stock-in and stock-out transactions
- Maintain movement history

### 6. Customer Management
- Create customers
- View customer details
- Update customer information
- Manage customer records

### 7. Customer Orders
- Create customer orders
- Add multiple products to orders
- Manage order items
- Track order information

### 8. Reports
- Dashboard summary
- Order status summary
- Monthly order summary
- Low-stock product report
- Recent inventory movements

### 9. Live Market
- Binance WebSocket integration
- Real-time BTCUSDT price updates
- Real-time ETHUSDT price updates
- Real-time BNBUSDT price updates
- 24-hour price change information
- High and low price information
- SignalR real-time broadcasting to Angular

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API |
| Framework | .NET 10 |
| Frontend | Angular |
| Language | C# / TypeScript |
| Database | Microsoft SQL Server |
| ORM | Entity Framework Core |
| Authentication | JWT |
| Real-time | SignalR |
| Market Data | Binance WebSocket |
| API Testing | Swagger / OpenAPI |
| IDE | Visual Studio / Visual Studio Code |

## 🏗️ Project Architecture

```text
SmartStock/
│
├── SmartStock.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
│
├── SmartStock.Domain/
│   ├── Entities/
│   └── Enums/
│
├── SmartStock.Infrastructure/
│   ├── Data/
│   ├── Configurations/
│   ├── Services/
│   └── Market/
│
├── SmartStock.Tests/
│
├── SmartStock.Web/
│
├── smartstock-ui/
│   ├── public/
│   ├── src/
│   │   └── app/
│   │       ├── core/
│   │       ├── features/
│   │       ├── layouts/
│   │       └── shared/
│   ├── angular.json
│   ├── package.json
│   └── README.md
│
├── SmartStock.slnx
└── README.md
```

## ⚙️ Prerequisites

Install the following before running the application:

- .NET 10 SDK
- Node.js (compatible with your Angular version)
- Angular CLI
- PostgreSQL
- Visual Studio 2026
- Visual Studio Code

## 🔧 Backend Setup

**1. Clone the repository**

```bash
git clone https://github.com/PrashanthCoding/SmartStock.git
```

**2. Navigate to the backend project**

Open the solution:

```text
SmartStock.slnx
```

**3. Configure PostgreSQL**

Update the connection string in the backend configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SmartStockDB;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Replace the example credentials with your local PostgreSQL credentials.

**4. Apply database migrations**

Run the following commands if migrations are configured in your project:

```bash
dotnet ef database update
```

**5. Run the backend**

```bash
dotnet run
```

Check the API URL and Swagger URL in your backend launch settings.

## 🎨 Frontend Setup

**1. Navigate to Angular project**

```bash
cd smartstock-ui
```

**2. Install dependencies**

```bash
npm install
```

**3. Run Angular application**

```bash
ng serve
```

**4. Open the application**

```text
http://localhost:4200
```

## 🔌 Real-Time Market Integration

SmartStock uses Binance WebSocket streams to receive real-time 24-hour ticker information.

Supported trading pairs:

- BTCUSDT
- ETHUSDT
- BNBUSDT

Market updates are received by the .NET background service and broadcast to connected Angular clients using SignalR.

SignalR Hub endpoint:

```text
/hubs/market
```

## 🔐 Security

- JWT-based authentication
- Protected Angular routes
- HTTP authentication interceptor
- Configuration-based database connection strings
- Sensitive credentials should not be committed to source control

## 📂 GitHub Repository

**Source Code:**

https://github.com/PrashanthCoding/SmartStock

## 👨‍💻 Developer

**Prashanth M**

GitHub: https://github.com/PrashanthCoding

---

⭐ If you find this project interesting, feel free to explore the source code and provide feedback.

**SmartStock – Simplifying Inventory Management.**
