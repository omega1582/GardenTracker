FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GardenTracker.sln .
COPY GardenTracker.Core/GardenTracker.Core.csproj GardenTracker.Core/
COPY GardenTracker.Data/GardenTracker.Data.csproj GardenTracker.Data/
COPY GardenTracker.Services/GardenTracker.Services.csproj GardenTracker.Services/
COPY GardenTracker.Api/GardenTracker.Api.csproj GardenTracker.Api/

RUN dotnet restore GardenTracker.Api/GardenTracker.Api.csproj

COPY GardenTracker.Core/ GardenTracker.Core/
COPY GardenTracker.Data/ GardenTracker.Data/
COPY GardenTracker.Services/ GardenTracker.Services/
COPY GardenTracker.Api/ GardenTracker.Api/

RUN dotnet publish GardenTracker.Api/GardenTracker.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "GardenTracker.Api.dll"]
