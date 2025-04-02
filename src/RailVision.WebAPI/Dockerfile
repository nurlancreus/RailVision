# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the certificate into the container during the build stage
COPY ["./certificates/localhost.pfx", "/https/localhost.pfx"]

# Set environment variables to use the certificate
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=valverde021

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RailVision.WebAPI/RailVision.WebAPI.csproj", "src/RailVision.WebAPI/"]
COPY ["src/RailVision.Infrastructure/RailVision.Infrastructure.csproj", "src/RailVision.Infrastructure/"]
COPY ["src/RailVision.Application/RailVision.Application.csproj", "src/RailVision.Application/"]
COPY ["src/RailVision.Domain/RailVision.Domain.csproj", "src/RailVision.Domain/"]
RUN dotnet restore "./src/RailVision.WebAPI/RailVision.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/RailVision.WebAPI"
RUN dotnet build "./RailVision.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RailVision.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RailVision.WebAPI.dll"]