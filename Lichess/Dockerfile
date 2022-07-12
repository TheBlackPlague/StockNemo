FROM ghcr.io/theblackplague/stocknemo:3.0.0.0 as LAUNCH

LABEL org.opencontainers.image.source=https://github.com/TheBlackPlague/StockNemo

RUN apt-get update &&\
    apt-get install -y python3 python3-pip git 

RUN git clone https://github.com/ShailChoksi/lichess-bot.git /lcbot

WORKDIR "/src/Terminal"
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /lcbot/engines  \
    /p:PublishSingleFile=true --self-contained true

WORKDIR "/lcbot"
COPY ["config.yml", "config.yml"]
COPY ["Perfect2021.bin", "engines/"]
RUN pip3 install wheel &&\
    pip3 install -r requirements.txt

CMD ["/lcbot/lichess-bot.py", "-v"]
ENTRYPOINT ["python3"]