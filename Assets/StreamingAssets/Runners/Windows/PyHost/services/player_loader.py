import asyncio
from concurrent.futures import CancelledError
from PyHost.services.python_runner import PythonRunner

class PlayerLoader:
    async def load_player(self, player_file_path, cancellation_token=None):
        try:
            return await asyncio.to_thread(self._load_player, player_file_path)
        except CancelledError:
            print("The task has been cancelled.")
            return None

    def _load_player(self, player_file_path):
        player = PythonRunner()
        player.setup(player_file_path)
        return player