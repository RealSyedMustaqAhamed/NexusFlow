FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NexusFlow.sln", "./"]
COPY ["src/NexusFlow.Domain/NexusFlow.Domain.csproj", "src/NexusFlow.Domain/"]
COPY ["src/NexusFlow.Application/NexusFlow.Application.csproj", "src/NexusFlow.Application/"]
COPY ["src/NexusFlow.Infrastructure/NexusFlow.Infrastructure.csproj", "src/NexusFlow.Infrastructure/"]
COPY ["src/NexusFlow.Api/NexusFlow.Api.csproj", "src/NexusFlow.Api/"]
COPY ["tests/NexusFlow.Tests/NexusFlow.Tests.csproj", "tests/NexusFlow.Tests/"]

RUN dotnet restore NexusFlow.sln

COPY . .
WORKDIR "/src/src/NexusFlow.Api"
RUN dotnet publish "NexusFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusFlow.Api.dll"]
