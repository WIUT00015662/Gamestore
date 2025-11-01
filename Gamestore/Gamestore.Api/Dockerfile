FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Gamestore/Gamestore.Api/Gamestore.Api.csproj Gamestore/Gamestore.Api/
COPY Gamestore/Gamestore.BLL/Gamestore.BLL.csproj Gamestore/Gamestore.BLL/
COPY Gamestore/Gamestore.DAL/Gamestore.DAL.csproj Gamestore/Gamestore.DAL/
COPY Gamestore/Gamestore.Domain/Gamestore.Domain.csproj Gamestore/Gamestore.Domain/
RUN dotnet restore Gamestore/Gamestore.Api/Gamestore.Api.csproj

COPY . .
RUN dotnet publish Gamestore/Gamestore.Api/Gamestore.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Gamestore.Api.dll"]
