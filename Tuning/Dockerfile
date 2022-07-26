FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dotnetsdk

RUN apt-get update -y && apt-get install -y wget git

RUN wget https://github.com/cutechess/cutechess/releases/download/1.2.0/cutechess-cli-1.2.0-linux64.tar.gz

RUN tar -xf cutechess-cli-1.2.0-linux64.tar.gz

RUN mkdir Tune

COPY ["UHO_XXL_+0.90_+1.19.epd", "Tune/UHO_XXL_+0.90_+1.19.epd"]
COPY ["tune.json", "Tune/tune.json"]

ARG BASE=master
ARG TUNE=tuning

RUN git clone https://github.com/TheBlackPlague/StockNemo.git

WORKDIR /StockNemo/Terminal

RUN git checkout $BASE
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /Tune  \
    /p:PublishSingleFile=true --self-contained true
RUN mv /Tune/StockNemo /Tune/StockNemoNOTUNE

RUN git checkout $PATCH
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /Tune  \
    /p:PublishSingleFile=true --self-contained true
RUN mv /Tune/StockNemo /Tune/StockNemoTUNE

FROM nvidia/cuda:11.0-runtime-ubuntu20.04 AS cuda

RUN rm /etc/apt/sources.list.d/cuda.list
RUN apt-key del 7fa2af80
RUN apt-key adv --fetch-keys https://developer.download.nvidia.com/compute/cuda/repos/ubuntu2004/x86_64/7fa2af80.pub

COPY --from=dotnetsdk ["Tune", "Tune"]
COPY --from=dotnetsdk ["cutechess-cli", "cutechess-cli"]

ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update -y --allow-unauthenticated && apt-get install -y python3 python3-pip libqt5core5a

ENV PATH=$PATH:/cutechess-cli
WORKDIR /Tune

RUN python3 -m pip install pip --upgrade && python3 -m pip install chess-tuning-tools
