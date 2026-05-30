FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "OleleLibraryNowAPI/OleleLibraryNowAPI.csproj"
RUN dotnet publish  "OleleLibraryNowAPI/OleleLibraryNowAPI.csproj" -c Release -o /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "OleleLibraryNowAPI.dll"]
