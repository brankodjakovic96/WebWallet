FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src
COPY ["Core/ApplicationServices/Core.ApplicationServices.csproj", "Core/ApplicationServices/"]

COPY ["Core/Infrastructure/DataAccess/EfCoreDataAccess/Core.Infrastructure.DataAccess.EfCoreDataAccess.csproj", "Core/Infrastructure/DataAccess/"]

COPY ["Core/Infrastructure/Services/BrankoBankServiceMock/Core.Infrastructure.Services.BrankoBankServiceMock.csproj", "Core/Infrastructure/Services/BrankoBankServiceMock/"]

COPY ["Applications/WebWallet/Applications.WebWallet.csproj", "Applications/WebWallet/"]

RUN dotnet restore "Applications/WebWallet/Applications.WebWallet.csproj"
COPY . .
WORKDIR "/src/Applications/WebWallet"
RUN dotnet build "Applications.WebWallet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Applications.WebWallet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Applications.WebWallet.dll"]