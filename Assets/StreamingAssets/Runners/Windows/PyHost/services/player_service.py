import asyncio

class PlayerService:
    def __init__(self, player_loader):
        self._player_loader = player_loader
        self._player_bag = {}

    def get_player(self, position):
        if position not in self._player_bag:
            raise Exception("no player by this position.")
        return self._player_bag[position]

    async def load_player(self, position, file_path, cancellation_token=None):
        player = await self._player_loader.load_player(file_path, cancellation_token)
        if player is None:
            raise Exception("Can not load player by this filePath. path: " + file_path)

        self._player_bag[position] = player

    def clean_up(self):
        self._player_bag.clear()