from PyHost.models.game_set import GameSet
import asyncio
import time


class GameService:
    def __init__(self, player_service, game_logger):
        self._player_service = player_service
        self._game_logger = game_logger
        self._gameId = ""
        self._column = 0
        self._row = 0
        self._total_packet_size = 0

    def init_game(self, gameId, column, row):
        self._gameId = gameId
        self._column = column
        self._row = row
        map_packet_size = (column * row) * 4
        self._total_packet_size = map_packet_size + 4

        self._game_logger.init(gameId, column, row)

    async def load_player(self, position, file_path, cancellation_token=None):
        await self._player_service.load_player(position, file_path, cancellation_token)

    def get_player_name(self, position):
        player = self._player_service.get_player(position)
        if player:
            return player.get_name()
        raise Exception(f"{position} Player name is null")

    def initialize_player(self, position, column, row):
        player = self._player_service.get_player(position)
        if player:
            player.initialize(position, column, row)
        else:
            raise Exception(f"Player at position {position} not found")

    async def move_next(self, message, cancellation_token=None):
        try:
            turn = message.turn
            position = message.position
            map_data = message.map
            current = message.current

            return await asyncio.to_thread(self._move_next_sync, turn, position, map_data, current)
        except Exception as ex:
            self._game_logger.log(f"Error during move_next: {ex}")
            self._game_logger.log_player_action(
                {"turn": turn, "position": position, "map": map_data, "current": current})
            return -1
            #raise  Exception(f"Error during move_next: {ex}")

    def _move_next_sync(self, turn, position, map_data, current):
        player = self._player_service.get_player(position)
        if player is None:
            return -1
            #raise Exception(f"Player at position {position} not found")

        direction = player.move_next(map_data, current)

        self._game_logger.log_player_action(
            {"turn": turn, "position": position, "map": map_data, "current": current, "direction": direction})

        if direction < 0 or direction > 3:
            return -1
            #raise Exception(f"The result is out of range. result: {direction}")

        return direction

    def get_current_game_set(self):
        return GameSet(column=self._column, row=self._row)

    def clean_up(self):
        self._column = 0
        self._row = 0
        self._player_service.clean_up()
        self._game_logger.cleanup()