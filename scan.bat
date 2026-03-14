dotnet sonarscanner begin /k:"BookStoreManagement" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="sqp_c0b00edb81ed69781d709406a41abbf9e2b00174"

dotnet build

dotnet sonarscanner end /d:sonar.token="sqp_c0b00edb81ed69781d709406a41abbf9e2b00174"