#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# rembg
RUN apt-get update -y
RUN apt-get install python3.9 -y
RUN apt-get install pip -y
RUN pip install rembg

RUN mkdir -p ~/.u2net
RUN apt-get install wget -y
RUN wget https://github.com/danielgatis/rembg/releases/download/v0.0.0/u2netp.onnx -O ~/.u2net/u2netp.onnx
RUN wget https://github.com/danielgatis/rembg/releases/download/v0.0.0/u2net.onnx -O ~/.u2net/u2net.onnx
RUN wget https://github.com/danielgatis/rembg/releases/download/v0.0.0/u2net_human_seg.onnx -O ~/.u2net/u2net_human_seg.onnx
RUN wget https://github.com/danielgatis/rembg/releases/download/v0.0.0/u2net_cloth_seg.onnx -O ~/.u2net/u2net_cloth_seg.onnx

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["rembg-api/rembg-api.csproj", "rembg-api/"]
RUN dotnet restore "rembg-api/rembg-api.csproj"
COPY . .
WORKDIR "/src/rembg-api"
RUN dotnet build "rembg-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "rembg-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "rembg-api.dll"]