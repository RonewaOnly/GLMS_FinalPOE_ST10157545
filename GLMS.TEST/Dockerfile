# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GLMS.TEST/GLMS.TEST.csproj", "GLMS.TEST/"]
COPY ["GLMS.API/GLMS.API.csproj", "GLMS.API/"]
COPY ["GLMS.Shared/GLMS.Shared.csproj", "GLMS.Shared/"]
COPY ["GLMS.Web/GLMS.Web.csproj", "GLMS.Web/"]
RUN dotnet restore "./GLMS.TEST/GLMS.TEST.csproj"
COPY . .
WORKDIR "/src/GLMS.TEST"
RUN dotnet build "./GLMS.TEST.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GLMS.TEST.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GLMS.TEST.dll"]