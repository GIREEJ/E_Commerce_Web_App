# Use the .NET 8 SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj and restore
COPY ECommerceWebApp/ECommerceWebApp.csproj ./ECommerceWebApp/
RUN dotnet restore ./ECommerceWebApp/ECommerceWebApp.csproj

# Copy the rest of the code
COPY ECommerceWebApp ./ECommerceWebApp

WORKDIR /src/ECommerceWebApp
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerceWebApp.dll"]
