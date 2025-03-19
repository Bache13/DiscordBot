# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy csproj and NuGet.Config files first for caching
COPY *.csproj ./
COPY NuGet.Config ./

# Restore packages (this will use our NuGet.Config settings)
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Publish the application; Disable fallback folders explicitly
RUN dotnet publish -c Release -o out /p:DisablePackageFallbackFolders=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
