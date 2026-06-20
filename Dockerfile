FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TKT.Api/TKT.Api.csproj TKT.Api/
COPY TKT.Core/TKT.Core.csproj TKT.Core/
COPY TKT.Infrastructure/TKT.Infrastructure.csproj TKT.Infrastructure/
RUN dotnet restore TKT.Api/TKT.Api.csproj
COPY . .
RUN dotnet publish TKT.Api/TKT.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TKT.Api.dll"]
