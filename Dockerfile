# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ECommerceWebApp.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/out

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Run app, binding to Render’s injected $PORT
CMD ["dotnet", "ECommerceWebApp.dll", "--urls", "http://0.0.0.0:$PORT"]
