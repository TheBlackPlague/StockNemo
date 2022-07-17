all: publish

ARCHITECTURE = linux-x64

ifeq ($(OS), Windows_NT)
	ARCHITECTURE = win-x64
endif

publish:
	dotnet publish "Terminal.csproj" -c Release -r $(ARCHITECTURE) -o ./  \
    /p:PublishSingleFile=true --self-contained true