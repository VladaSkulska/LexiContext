# 1. Етап збірки (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо файл рішення та всі 5 .csproj файлів відповідно до твого Solution Explorer
COPY *.sln ./
COPY LexiContext.API/*.csproj ./LexiContext.API/
COPY LexiContext.Application/*.csproj ./LexiContext.Application/
COPY LexiContext.Domain/*.csproj ./LexiContext.Domain/
COPY LexiContext.Infrastructure/*.csproj ./LexiContext.Infrastructure/

# Відновлюємо залежності для всіх проектів
RUN dotnet restore

# Копіюємо абсолютно всі файли коду в контейнер
COPY . .

# Переходимо в папку головного API проекту і публікуємо його в папку /app/publish
WORKDIR "/src/LexiContext.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 2. Етап запуску (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Налаштовуємо стандартний порт Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LexiContext.API.dll"]