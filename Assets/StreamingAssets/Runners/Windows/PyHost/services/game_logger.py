import os
import logging
import json
from typing import Dict, Optional


class GameLogger:
    GAME_LOG_NAME = "game.log"
    JSON_SETTINGS = {"indent": 4}

    def __init__(self, log_root):
        self._log_root = log_root
        self._log_path: Optional[str] = None
        self._current_game: Optional[str] = None
        self._game_logger: Optional[logging.Logger] = None
        self._player_loggers: Dict[int, logging.Logger] = {}

    @property
    def root_logger(self) -> logging.Logger:
        return self._game_logger or self.get_default_logger()

    def init(self, game_id: str, column: int, row: int):
        self._log_path = os.path.join(self._log_root, game_id)
        self._current_game = game_id
        os.makedirs(self._log_path, exist_ok=True)

        log_file_path = os.path.join(self._log_path, self.GAME_LOG_NAME)
        self._game_logger = self._create_logger(log_file_path, game_id)

        message = f"Game initialized. GameId: {game_id}, column: {column}, row: {row}"
        self._game_logger.info(message)

    def log(self, message: str):
        self.root_logger.info(message)

    def log_player_action(self, game_turn_info):
        player = game_turn_info["position"]
        player_logger = self._get_or_create_player_logger(player)
        player_logger.info(json.dumps(game_turn_info, **self.JSON_SETTINGS))

    def log_result(self, result):
        result_string = json.dumps(result, **self.JSON_SETTINGS)
        message = f"Game set.\n{result_string}"
        self.root_logger.info(message)

    def cleanup(self):
        self._player_loggers.clear()

    def get_default_logger(self) -> logging.Logger:
        logger = logging.getLogger("default")
        if not logger.handlers:
            handler = logging.StreamHandler()
            formatter = logging.Formatter("[{asctime}] {message}", style="{")
            handler.setFormatter(formatter)
            logger.addHandler(handler)
        logger.setLevel(logging.INFO)
        return logger

    def _create_logger(self, log_file_path: str, game_id: Optional[str] = None) -> logging.Logger:
        logger = logging.getLogger(log_file_path)
        if not logger.handlers:
            handler = logging.FileHandler(log_file_path)
            formatter = logging.Formatter(
                fmt=f"[{game_id}:%(asctime)s] %(message)s",
                datefmt="%Y-%m-%d %H:%M:%S"  # 날짜 형식 지정
            )
            handler.setFormatter(formatter)
            logger.addHandler(handler)
        logger.setLevel(logging.INFO)
        return logger

    def _get_or_create_player_logger(self, player: int) -> logging.Logger:
        if not self._log_path:
            return self.get_default_logger()

        if player not in self._player_loggers:
            player_log_path = os.path.join(self._log_path, f"{player}.log")
            self._player_loggers[player] = self._create_logger(player_log_path, self._current_game)

        return self._player_loggers[player]
