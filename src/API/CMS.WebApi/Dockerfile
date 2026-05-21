FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# csproj copy
COPY ["src/API/CMS.WebApi/CMS.WebApi.csproj", "src/API/CMS.WebApi/"]
COPY ["src/API/CMS.Application/CMS.Application.csproj", "src/API/CMS.Application/"]
COPY ["src/API/CMS.Domain/CMS.Domain.csproj", "src/API/CMS.Domain/"]
COPY ["src/API/CMS.Infrastructure/CMS.Infrastructure.csproj", "src/API/CMS.Infrastructure/"]
COPY ["src/API/CMS.SharedKernel/CMS.SharedKernel.csproj", "src/API/CMS.SharedKernel/"]

# restore (DOĞRU PATH)
RUN dotnet restore "src/API/CMS.WebApi/CMS.WebApi.csproj"

# tüm source
COPY . .

# WORKDIR FIX
WORKDIR /src/src/API/CMS.WebApi

# build
RUN dotnet build "CMS.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CMS.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CMS.WebApi.dll"]