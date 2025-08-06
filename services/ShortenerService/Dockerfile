FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShortenerService.csproj","ShortenerService/"]
WORKDIR /src/ShortenerService
RUN dotnet restore "ShortenerService.csproj"
COPY . ./
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS Publish
ARG BUILD_CONFIGURATION=Release 
RUN dotnet publish "ShortenerService.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore -p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=Publish /app/publish .
ENTRYPOINT ["dotnet", "ShortenerService.dll"]