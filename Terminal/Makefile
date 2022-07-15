all: publish

publish:
	dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /chess/engines  \
    /p:PublishSingleFile=true --self-contained true