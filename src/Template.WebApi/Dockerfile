FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Template.Domain/Template.Domain.csproj", "src/Template.Domain/"]
COPY ["src/Template.Application/Template.Application.csproj", "src/Template.Application/"]
COPY ["src/Template.Infrastructure/Template.Infrastructure.csproj", "src/Template.Infrastructure/"]
COPY ["src/Template.WebApi/Template.WebApi.csproj", "src/Template.WebApi/"]
RUN dotnet restore "src/Template.WebApi/Template.WebApi.csproj"
COPY . .
RUN dotnet publish "src/Template.WebApi/Template.WebApi.csproj" -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Template.WebApi.dll"]
