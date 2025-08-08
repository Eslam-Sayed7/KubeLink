FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthService.csproj","AuthService/"]
WORKDIR /src/AuthService
RUN dotnet restore "AuthService.csproj"
COPY . ./
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS Publish
ARG BUILD_CONFIGURATION=Release 
RUN dotnet publish "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore -p:PublishTrimmed=false

FROM base AS final
WORKDIR /app
COPY --from=Publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]
