# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChisaApi.Web/ChisaApi.Web.csproj ChisaApi.Web/
COPY ChisaApi.Application/ChisaApi.Application.csproj ChisaApi.Application/
COPY ChisaApi.Domain/ChisaApi.Domain.csproj ChisaApi.Domain/
COPY ChisaApi.Infrastructure/ChisaApi.Infrastructure.csproj ChisaApi.Infrastructure/

RUN dotnet restore ChisaApi.Web/ChisaApi.Web.csproj

COPY . .

RUN dotnet publish ChisaApi.Web/ChisaApi.Web.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ChisaApi.Web.dll"]
