import chess.pgn
import requests
import os

from rich.progress import Progress
from concurrent.futures import ThreadPoolExecutor

URL = "http://localhost:6175/game.gif"
name = "StockNemo"
version = "2.0.0.3"

print("Welcome to " + name + " PGN to GIF tool, please respond to inputs below...")
default_delay = 60
pgn_file_path = input("Please enter PGN file path: ")
pgn = open(pgn_file_path)
gif_output_path = input("Please enter directory to output GIF: ")


class Frame:
    def __init__(self, fen, frame_delay, last_move, red_sq):
        self.fen = fen
        self.delay = frame_delay
        self.last_move = last_move
        self.check_sq = red_sq

    def __str__(self):
        return self.fen + " " + str(self.delay) + " " + str(self.last_move) + " " + str(self.check_sq)

    def get_data(self):
        frame_data = {
            "fen": self.fen,
            "delay": self.delay,
            "lastMove": str(self.last_move),
            "check": self.check_sq
        }

        if self.last_move is None:
            frame_data.pop("lastMove")

        if self.check_sq is None:
            frame_data.pop("check")

        return frame_data


class ThreadedGameData:
    def __init__(self, game_to_convert, path):
        self.game = game_to_convert
        self.path = path


def game_to_gif(game_data: ThreadedGameData):
    board = game_data.game.board()
    frames = [Frame(board.board_fen(), default_delay * 3, None, None).get_data()]

    frame_id = 0
    for move in game_data.game.mainline_moves():
        board.push(move)

        check_sq = None
        if board.is_check():
            check_sq = chess.square_name(board.king(board.turn))

        frames.append(Frame(board.board_fen(), default_delay, move, check_sq).get_data())

        frame_id += 1

    data = {
        "white": game_data.game.headers["White"],
        "black": game_data.game.headers["Black"],
        "comment": "StockNemo PgnToGif",
        "orientation": "black" if game_data.game.headers["Result"] == "0-1" else "white",
        "delay": default_delay,
        "frames": frames
    }

    response = requests.post(url=URL, json=data)

    if os.path.isfile(game_data.path) is False:
        file = open(game_data.path, "x")
        file.close()

    with open(game_data.path, "wb") as file:
        file.write(response.content)


read_all = True if int(input("Please enter 0 for engine specific or 1 for all: ")) == 1 else False
engine = ""
if read_all:
    engine = input("Enter full engine name with version as it appears in PGN: ")

games = []
parsed_game = chess.pgn.read_game(pgn)

with Progress() as read_progress:
    read_task = read_progress.add_task("[green]Reading games...", total=2000)
    while parsed_game is not None:
        white = parsed_game.headers["White"]
        black = parsed_game.headers["Black"]
        if read_all and white != engine and black != engine:
            read_progress.advance(read_task, advance=1)
            parsed_game = chess.pgn.read_game(pgn)
            continue

        games.append(parsed_game)

        read_progress.advance(read_task, advance=1)

        parsed_game = chess.pgn.read_game(pgn)

worker_count = 64

print("Setting up thread pool...")

with ThreadPoolExecutor(max_workers=worker_count) as thread_pool:
    print("Thread pool ready.")
    with Progress() as progress:
        tasks = [
            ThreadedGameData(game, gif_output_path + "\\" + str(file_id) + ".gif") for file_id, game in enumerate(games)
        ]

        for _ in progress.track(thread_pool.map(game_to_gif, tasks),
                                description="[green]Generating GIFs: ",
                                total=len(tasks)
                                ):
            pass
