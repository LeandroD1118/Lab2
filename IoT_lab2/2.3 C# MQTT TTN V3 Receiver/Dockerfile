# https://hub.docker.com/_/microsoft-dotnet
# https://github.com/dotnet/dotnet-docker/tree/main/samples/dotnetapp
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# ubuntu-arm64
#FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy AS build
WORKDIR /source

# copy csproj and restore as distinct layers
#COPY *.csproj .
# copy all project folders and sub-files from the .SLN root folder
COPY . .
# and restore as distinct layers
RUN dotnet restore
#RUN dotnet restore -r linux-arm64

# copy and publish app and libraries, point out ENTRYPOINT
#COPY . .
RUN dotnet publish ConsoleAppLecture -c release -o /app --no-restore
#RUN dotnet publish ConsoleAppLecture -c release -o /app --no-restore --self-contained --runtime linux-arm64

# final stage, create image
FROM mcr.microsoft.com/dotnet/runtime:7.0
#FROM mcr.microsoft.com/dotnet/runtime:7.0-jammy-arm64v8
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ConsoleAppLecture.dll"]

# Build image with cache or not
# docker build --no-cache -t hajo66/iot-repo:lora-ttn-lecture -f Dockerfile .
# docker build -t hajo66/iot-repo:lora-ttn-lecture -f Dockerfile .

# Run interactive and remove container after stop
# docker run -it --rm --name lora-ttn-app hajo66/iot-repo:lora-ttn-lecture

# As above run but also use a volume: https://docs.docker.com/storage/volumes/
# docker run -v ////c/vm/conf:/vm/conf -it --rm --name lora-ttn-app hajo66/iot-repo:lora-ttn-lecture

# Publish on other host - How to copy Docker images from one host to another without using a repository
# https://stackoverflow.com/questions/23935141/how-to-copy-docker-images-from-one-host-to-another-without-using-a-repository
# docker save -o c:/tmp/lora-ttn.tar lora-ttn-lecture:latest
# docker load -i c:/tmp/lora-ttn.tar
