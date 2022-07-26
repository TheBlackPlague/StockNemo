FROM nvidia/cuda:11.7.0-runtime-ubuntu20.04 AS base

RUN apt-get update -y && apt-get install -y wget

RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    sudo dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb 

RUN apt-get update -y && apt-get install -y apt-transport-https dotnet-sdk-6.0 git libqt5core5a python3 python3-pip

RUN wget https://github.com/cutechess/cutechess/releases/download/1.2.0/cutechess-cli-1.2.0-linux64.tar.gz

RUN tar -xf cutechess-cli-1.2.0-linux64.tar.gz
RUN export PATH=$PATH:/cutechess-cli

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

WORKDIR /Tune

RUN python -m pip install pip --upgrade && python -m pip install chess-tuning-tools
RUN tune local -c tune.json

