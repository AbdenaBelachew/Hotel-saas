FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PMS.sln .
COPY src/PMS.Domain/PMS.Domain.csproj src/PMS.Domain/
COPY src/PMS.Application/PMS.Application.csproj src/PMS.Application/
COPY src/PMS.Infrastructure/PMS.Infrastructure.csproj src/PMS.Infrastructure/
COPY src/PMS.API/PMS.API.csproj src/PMS.API/
COPY tests/PMS.Tests/PMS.Tests.csproj tests/PMS.Tests/
RUN dotnet restore src/PMS.API/PMS.API.csproj

COPY . .
RUN dotnet publish src/PMS.API/PMS.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PMS.API.dll"]
