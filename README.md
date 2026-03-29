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

## Docker

Tek dosya: **`docker-compose.yml`**.

1. `.env.example` → `.env`. **`MSSQL_HOST`**, **`MSSQL_PASSWORD`** (ve gerekirse `MSSQL_USER`, `MSSQL_DATABASE`) — connection string’i uygulama `SqlConnectionStringBuilder` ile oluşturur; `;` / `=` içeren parolaları elle yazmana gerek kalmaz.
2. `docker compose up -d --build` — sadece API; SQL sizde (başka konteyner / sunucu).

**Bu repoda SQL de gelsin** (profil `bundled-db`):

```bash
docker compose --profile bundled-db up -d --build
```

Yerleşik SQL için `.env` örneğindeki gibi `MSSQL_HOST=sqlserver` ve `MSSQL_PASSWORD` ile `MSSQL_SA_PASSWORD` aynı olmalı.

İsterseniz `MSSQL_HOST` kullanmadan yalnızca **`ConnectionStrings__DefaultConnection`** ortam değişkeni de verebilirsiniz (`.env` içinde `MSSQL_HOST` satırını kaldırın veya boş bırakın).

**Mevcut `sqlserver` konteynerı** ile API aynı Docker kullanıcı tanımlı ağda değilse, konteyner adı çözülmez; gerekirine API konteynerını o ağa bağlayın: `docker network connect <ağ_adı> <epos_api_konteyneri>` (`docker inspect sqlserver` ile ağ adına bakın).

- API: `http://localhost:8081` (`API_HOST_PORT`)

**Sadece imaj**

```bash
docker build -t epos-api:latest .
docker run -p 8081:8080 -e MSSQL_HOST=sqlserver -e MSSQL_PASSWORD=... -e MSSQL_USER=sa epos-api:latest
```
