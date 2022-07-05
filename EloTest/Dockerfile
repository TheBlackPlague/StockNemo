FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

RUN apt-get update && apt-get update -y && apt-get install -y wget tar git libqt5core5a

RUN wget https://github.com/cutechess/cutechess/releases/download/1.2.0/cutechess-cli-1.2.0-linux64.tar.gz

RUN mkdir cutechess
RUN tar -xf cutechess-cli-1.2.0-linux64.tar.gz -C cutechess
RUN mkdir chess && mv /cutechess/cutechess-cli/cutechess-cli /chess/cli
RUN rm -r cutechess cutechess-cli-1.2.0-linux64.tar.gz
RUN mkdir /chess/engines

COPY ["UHO_XXL_+0.90_+1.19.epd", "/chess/UHO_XXL_+0.90_+1.19.epd"]

# Base image we're testing patch against.
ARG BASE_COMMIT_OR_TAG=master

# Patch image we're testing.
ARG PATCH_COMMIT_OR_TAG=2.0.0.2

RUN git clone https://github.com/TheBlackPlague/StockNemo.git

WORKDIR /StockNemo/Terminal

# First build base image.
RUN git checkout $BASE_COMMIT_OR_TAG
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /chess/engines  \
    /p:PublishSingleFile=true --self-contained true
RUN mv /chess/engines/StockNemo /chess/engines/StockNemo-$BASE_COMMIT_OR_TAG

# Next, we build patch image.
RUN git checkout $PATCH_COMMIT_OR_TAG
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /chess/engines  \
    /p:PublishSingleFile=true --self-contained true
RUN mv /chess/engines/StockNemo /chess/engines/StockNemo-$PATCH_COMMIT_OR_TAG

# Start the test.
WORKDIR /chess

ENV BASE=$BASE_COMMIT_OR_TAG
ENV PATCH=$PATCH_COMMIT_OR_TAG

ENTRYPOINT ./cli \
           -engine name=StockNemo-$PATCH cmd=engines/StockNemo-$PATCH \
           -engine name=StockNemo-$BASE cmd=engines/StockNemo-$BASE \
           -each arg="--uci=True" proto=uci tc=inf/10+0.1 option.Hash=64 \
           -openings file=UHO_XXL_+0.90_+1.19.epd format=epd order=random plies=6 -repeat \
           -rounds 10000 -games 2 -concurrency 64 \
           -sprt elo0=0 elo1=10 alpha=0.05 beta=0.05