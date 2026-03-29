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
