FROM ghcr.io/theblackplague/stocknemo:5.1.0.0 as LAUNCH

LABEL org.opencontainers.image.source=https://github.com/TheBlackPlague/StockNemo

RUN apt-get update &&\
    apt-get install -y python3 python3-pip git 

RUN git clone https://github.com/ShailChoksi/lichess-bot.git /lcbot

RUN mv /App/StockNemo /lcbot/engines/StockNemo

WORKDIR "/lcbot"
COPY ["config.yml", "config.yml"]
COPY ["StockNemoGM.bin", "engines/"]
RUN pip3 install wheel &&\
    pip3 install -r requirements.txt

CMD ["/lcbot/lichess-bot.py", "-v"]
ENTRYPOINT ["python3"]