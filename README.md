# Dlp.Connectors
Connector for database operations.

The Dlp.Connectors package provides a easy way to access a MSSQL database, being a simple and fast mapper to ADO.NET.

The following funcionality are available in this release:

## DatabaseConnector

  - BeginTransaction(): Starts a database transaction.
  - BulkInsert(): Copies all elements of a collection to a destination table.
  - Commit(): Commits the database transaction.
  - ExecuteNonQuery(): Executes the specified query and returns the number of affected rows.
  - ExecuteReader(): Sends the query to be executed and return a collection of rows already mapped to the specified object type.
  - ExecuteScalar(): Executes the query and returns the first column of the first row.
  - Rollback(): Rolls back a transaction from a pending state.

### Install from nuget.org

The official version can be obtained from the nuget package manager with the following command line:

**PM> Install-Package Dlp.Connectors.dll**
