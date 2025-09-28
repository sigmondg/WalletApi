# Currency Wallet API - Database Setup

## Prerequisites
- SQL Server (any edition) running on localhost

## Setup Instructions

1. **Run the Database Setup Script**
   - Open and execute `DatabaseSetup.sql`
   - The script will create:
     - Database: `CurrencyWalletDb`
     - All required tables with proper relationships
     - Optimized indexes for performance

2. **Run the Application**
   - Open the solution in Visual Studio or JetBrains Rider
   - Set `WalletApi` as the startup project
   - Run the application

## Connection Details
- **Server**: localhost
- **Database**: CurrencyWalletDb

## API Endpoints
- CreateWallet
- GetBalance
- AdjustBalance

## Notes
- All indexes are optimized for the expected query patterns
