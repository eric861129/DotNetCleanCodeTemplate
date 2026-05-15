FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Template.Domain/Template.Domain.csproj", "src/Template.Domain/"]
COPY ["src/Template.Application/Template.Application.csproj", "src/Template.Application/"]
COPY ["src/Template.Infrastructure/Template.Infrastructure.csproj", "src/Template.Infrastructure/"]
COPY ["src/Template.Worker/Template.Worker.csproj", "src/Template.Worker/"]
RUN dotnet restore "src/Template.Worker/Template.Worker.csproj"
COPY . .
RUN dotnet publish "src/Template.Worker/Template.Worker.csproj" -c Release -o /app/publish --no-restore

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Template.Worker.dll"]
