.PHONY: all clean

all: ipk24chat-server

ipk24chat-server:
	rm -f ./ipk24chat-server
	dotnet publish ipk24chat-server.csproj -r linux-x64 -c Release -o . -p:PublishSingleFile=true --self-contained true

clean:
	rm -f ./ipk24chat-server
