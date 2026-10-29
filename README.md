# FileProcessor

FileProcessor is an ASP.NET Core Web API that accepts CSV uploads, validates valid file type as CSV and counts processed and skipped rows, and exposes an in-memory processing report.

## Requirements

- .NET 10 SDK
- Docker Desktop
- An API key configured before the application starts

## Run locally

From the repository root:

```powershell
dotnet run --project .\FileProcessor\FileProcessor.csproj
```

The development launch profile listens on `http://localhost:5250`.
When running in Docker, replace `http://localhost:5250` with `http://localhost:8080`.


## Configuration

For local development, set the API key in `FileProcessor/appsettings.Development.json`:

```powershell

{
  "ApiSecurity": {
	"ApiKey": "replace-with-a-local-api-key"
  }
}
```

The default API-key header is `X-Api-Key`. The header name can be changed through `ApiSecurity:HeaderName`.

## API

### Health check

`GET /health`

This endpoint does not require an API key.

```powershell
Invoke-RestMethod http://localhost:5250/health
```

### Upload a CSV file

`POST /api/files`

The request must be `multipart/form-data` with a form field named `file`. Only non-empty `.csv` files up to 1 MiB are accepted. The CSV header must contain a `FullName` column.

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5250/api/files `
  -Method Post `
  -Headers @{ "X-Api-Key" = "replace-with-a-local-api-key" } `
  -Form @{ file = Get-Item .\FileProcessor\SampleFile\Sample-Claims.csv }
```

A successful response contains the file name, processed-row count, skipped-row count, and processing time in milliseconds. Rows that do not contain the `FullName` column position are skipped.

### Processing report

`GET /api/files/report`

Returns the files processed since the application started, along with aggregate processing time. The report is stored in memory and is cleared when the application restarts.

```powershell
Invoke-RestMethod `
  -Uri http://localhost:5250/api/files/report `
  -Headers @{ "X-Api-Key" = "replace-with-a-local-api-key" }
```

## API documentation

Swagger UI is available at:

- `http://localhost:5250/swagger`
