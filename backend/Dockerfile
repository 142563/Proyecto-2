FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Academic.sln ./
COPY Directory.Build.props ./
COPY src/Academic.Domain/Academic.Domain.csproj src/Academic.Domain/
COPY src/Academic.Application/Academic.Application.csproj src/Academic.Application/
COPY src/Academic.Infrastructure/Academic.Infrastructure.csproj src/Academic.Infrastructure/
COPY src/Academic.Api/Academic.Api.csproj src/Academic.Api/

RUN dotnet restore src/Academic.Api/Academic.Api.csproj

COPY . .
RUN dotnet publish src/Academic.Api/Academic.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Academic.Api.dll"]
