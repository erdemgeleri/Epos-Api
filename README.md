# EposDashboard API - Local Calistirma Rehberi

Bu proje ASP.NET Core 9 Web API uygulamasidir.

## Ozellikler

- JWT tabanli kimlik dogrulama ve rol bazli yetkilendirme (Admin, Isletme, Musteri)
- Isletme, urun, siparis ve kullanici yonetimi icin REST API endpointleri
- SignalR ile gercek zamanli olaylar (siparis, urun degisimi, sohbet)
- Isletme ve musteri arasinda canli sohbet yapisi (conversation ve typing olaylari)
- Entity Framework Core + SQL Server ile veri katmani
- Uygulama acilisinda migration ve demo veri seed islemleri
- Gelistirme ortaminda OpenAPI dokumani (`/openapi/v1.json`)

## Gereksinimler

- .NET SDK 9.0
- SQL Server (LocalDB, SQL Server Express veya SQL Server Developer)

## 1) Veritabani baglantisini kontrol et

Varsayilan baglanti dizesi `appsettings.json` dosyasindadir:

`Server=localhost;Database=PosDemoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

SQL Server instance'in farkliysa `appsettings.json` icindeki `ConnectionStrings:DefaultConnection` degerini guncelle.

## 2) Projeyi calistir

Proje klasorunde terminal acip calistir:

```bash
dotnet restore
dotnet run
```

Uygulama acilista migrationlari uygular ve demo verileri otomatik ekler.

## 3) API'nin ayakta oldugunu dogrula

Tarayicida veya Postman ile asagidaki endpoint'i ac:

- `GET /openapi/v1.json`

Calistigi anda terminalde `Now listening on ...` satirini gorursun.

## Demo giris bilgileri

Seed edilen hazir kullanicilar:

- Admin: `admin@posdemo.local`
- Isletme: `business@posdemo.local`
- Musteri: `customer@posdemo.local`
- Sifre: `PosDemo2026!`

## Notlar

- CORS ayari local test icin acik durumdadir.
- `appsettings.Development.json` lokal ortam log seviyelerini icerir.
