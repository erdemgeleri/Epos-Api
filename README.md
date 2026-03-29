# Epos-Api

ASP.NET Core 9 Web API (`EposDashboard.Api.csproj`) — çözüm dosyası ile aynı klasörde.

## Klasör yapısı

| Yol | Açıklama |
|-----|----------|
| `EposDashboard.Api.sln` | Visual Studio çözümü |
| `EposDashboard.Api.csproj` | API projesi |
| `Controllers/`, `Data/`, … | Uygulama kodu |
| `tools/HashPassword/` | İsteğe bağlı: şifre hash CLI (`dotnet run --project tools/HashPassword`) |

`tools` ana projeden **hariç tutulur** (`csproj` içinde `Compile Remove`); API ile karışmaz.

## Çalıştırma

```bash
dotnet run
```

## Docker (SQL Server + API)

1. `.env.example` dosyasını `.env` olarak kopyalayın ve `MSSQL_SA_PASSWORD`, `JWT_SECRET` değerlerini üretim için değiştirin.
2. Repo kökünde:

```bash
docker compose up -d --build
```

- API: `http://localhost:8080`
- SQL Server: host’tan `localhost:1433` (SA şifresi `.env` ile aynı)

Sadece API imajı:

```bash
docker build -t epos-api:latest .
```

Harici veritabanı kullanırken container’ı `ConnectionStrings__DefaultConnection` ortam değişkeni ile çalıştırın; `docker-compose.yml` içindeki `sqlserver` servisini kaldırabilirsiniz.
