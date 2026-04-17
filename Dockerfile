# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/AzureWatcher.Function/AzureWatcher.Function.csproj", "AzureWatcher.Function/"]
RUN dotnet restore "AzureWatcher.Function/AzureWatcher.Function.csproj"

# Copy everything else and build the app
COPY src/AzureWatcher.Function/ AzureWatcher.Function/
WORKDIR "/src/AzureWatcher.Function"
RUN dotnet publish "AzureWatcher.Function.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes the container via a dynamic port, though Background Workers don't strictly need it.
# EXPOSE 80 
ENTRYPOINT ["dotnet", "AzureWatcher.Function.dll"]
