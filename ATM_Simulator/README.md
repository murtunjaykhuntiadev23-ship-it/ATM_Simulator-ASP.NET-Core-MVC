# SimpleATM - ATM Simulator

A web-based ATM simulator built with **ASP.NET Core MVC**, **Entity Framework Core**, **SQL Server** and **ASP.NET Core Identity**.

## Features
- User registration, login and logout (ASP.NET Core Identity, cookie authentication)
- Create an ATM account with a 4-digit PIN (PIN is stored hashed, never as plain text)
- Auto-generated 10-digit account number
- Deposit and withdraw money (PIN required, cannot withdraw more than the balance)
- Mini statement showing the last 10 transactions
- Change PIN
- Account locks for 15 minutes after 3 wrong PIN attempts
- Each user can only see and use their own account

## Tech Stack
- ASP.NET Core MVC (.NET)
- Entity Framework Core (Code First, Migrations)
- SQL Server LocalDB
- Bootstrap

## How to Run
1. Clone the repository
2. Open the solution in Visual Studio
3. Open a terminal in the project folder and run: `dotnet ef database update`
4. Press F5 to run
5. Register a user, click the confirm link, log in, then open the ATM page

## Security
- Passwords and PINs are hashed
- `[Authorize]` protects the ATM pages
- Anti-forgery tokens on all forms
- PIN lockout after 3 failed attempts

## Author
Murtunjay Khuntia