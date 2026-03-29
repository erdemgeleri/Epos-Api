# Build context: bu repo kökü (EposDashboard.Api.csproj ile aynı klasör).
#   docker build -t epos-api:latest .
#   docker run -p 8081:8080 -e ConnectionStrings__DefaultConnection=... epos-api:latest

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY EposDashboard.Api.csproj ./
RUN dotnet restore EposDashboard.Api.csproj
COPY . ./
RUN dotnet publish EposDashboard.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EposDashboard.Api.dll"]
