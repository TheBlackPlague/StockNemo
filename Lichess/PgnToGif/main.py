import chess.pgn
import requests
import os

URL = "http://localhost:6175/game.gif"
name = "StockNemo"
version = "2.0.0.3"

print("Welcome to " + name + " PGN to GIF tool, please respond to inputs below...")
default_delay = 60
pgn_file_path = input("Please enter PGN file path: ")
pgn = open(pgn_file_path)
gif_output_path = input("Please enter directory to output GIF: ")
print(gif_output_path)

if os.path.isdir(gif_output_path) is False:
    print("Path provided is not a directory.")
    exit(1)


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


def read_game(parse_id):
    print("READING GAME [ " + str(parse_id) + " ] ...")
    pgn_game = chess.pgn.read_game(pgn)
    print("GAME [ " + str(parse_id) + " ] READ SUCCESSFULLY.")
    return pgn_game


engine = name + " " + version
game_id = 0
file_id = 0
game = read_game(game_id)
while game is not None:
    white = game.headers["White"]
    black = game.headers["Black"]
    if white != engine and black != engine:
        print("Non " + engine + " game found! Skipping...")
        game_id += 1
        game = read_game(game_id)
        continue

    board = game.board()
    frames = [Frame(board.board_fen(), default_delay * 3, None, None).get_data()]

    frame_id = 0
    for move in game.mainline_moves():
        board.push(move)

        check_sq = None
        if board.is_check():
            check_sq = chess.square_name(board.king(board.turn))

        frames.append(Frame(board.board_fen(), default_delay, move, check_sq).get_data())
        print("GENERATED FRAME [ " + str(frame_id) + " ]")
        frame_id += 1

    print("Forwarding frames to LILA-GIF...")
    data = {
        "white": white,
        "black": black,
        "comment": "StockNemo PgnToGif",
        "orientation": "black" if game.headers["Result"] == "0-1" else "white",
        "delay": default_delay,
        "frames": frames
    }

    response = requests.post(url=URL, json=data)
    if response.status_code != 200:
        print("Error [ " + str(response.status_code) + " ]: " + response.text)
    else:
        print("LILA-GIF returned GIF data.")

    gif_path = gif_output_path + "/" + str(file_id) + ".gif"

    print("Saving GIF to: " + gif_path)
    if os.path.isfile(gif_path) is False:
        file = open(gif_path, "x")
        file.close()

    file = open(gif_path, "wb")

    file.write(response.content)

    file.close()
    print("GIF saved successfully.")

    game_id += 1
    game = read_game(game_id)
    file_id += 1
