# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy only csproj first (for cache efficiency)
COPY ECommerceWebApp.csproj ./
RUN dotnet restore

# copy the rest of the code
COPY . ./
RUN dotnet publish -c Release -o /app/out

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# copy published app
COPY --from=build /app/out ./

# Render sets $PORT dynamically, so bind to it
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "ECommerceWebApp.dll"]
