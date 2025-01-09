FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /Meetme.AuthService
COPY ["Meetme.AuthService/Meetme.AuthService.API/Meetme.AuthService.API.csproj", "Meetme.AuthService.API/"]
COPY ["Meetme.AuthService/Meetme.AuthService.BLL/Meetme.AuthService.BLL.csproj", "Meetme.AuthService.BLL/"]
RUN dotnet restore "Meetme.AuthService.API/Meetme.AuthService.API.csproj"
COPY . /
WORKDIR /Meetme.AuthService/Meetme.AuthService.API
RUN dotnet build "Meetme.AuthService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_HTTP_PORTS=5041
EXPOSE 5041
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Meetme.AuthService.API.dll"]
