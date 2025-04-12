FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY dotnet_chatroom_service/*.csproj ./dotnet_chatroom_service/
COPY Common/*.csproj ./Common/
COPY Repositories/*.csproj ./Repositories/
COPY Services/*.csproj ./Services/
COPY dotnet_chatroom_service/appsettings.Secrets.json ./dotnet_chatroom_service/

RUN dotnet restore "dotnet_chatroom_service/dotnet_chatroom_service.csproj"

COPY . .

RUN dotnet build "dotnet_chatroom_service/dotnet_chatroom_service.csproj" \
    --configuration Release \
    --no-restore \
    -p:ExcludeTests=true

RUN dotnet publish "dotnet_chatroom_service/dotnet_chatroom_service.csproj" \
    --configuration Release \
    --no-build \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DotNet_ConnectionStrings__DefaultConnection=unset \
    DotNet_JwtConfig__SecretKey=unset

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "dotnet_chatroom_service.dll"]