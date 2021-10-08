Copy-Item "$env:Appdata\NuGet\NuGet.Config" -Destination ".\nuget.config"
docker-compose up -d