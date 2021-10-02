#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# => run commends in solution directory
#docker build -f "PROJECT_PATH\Dockerfile" -t IMAGE_NAME:dev "SOLUTION_PATH"
#docker build -f ".\MinimalApi\Dockerfile" -t minimal-api:dev "."

#docker run -p 5000:80 minimal-api:dev

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MinimalApi/MinimalApi.csproj", "MinimalApi/"]
RUN dotnet restore "MinimalApi/MinimalApi.csproj"
COPY . .
WORKDIR "/src/MinimalApi"
RUN dotnet build "MinimalApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MinimalApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinimalApi.dll"]