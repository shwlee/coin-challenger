from PyHost.services.player_factory import PlayerFactory

class PythonRunner:
    def __init__(self):
        self._get_name = None
        self._initialize = None
        self._move_next = None
        self._running_instance = None

    def setup(self, path):
        player_class, instance = PlayerFactory.load_code_module(path)
        if player_class is None:
            raise Exception(f"player class is null. path: {path}")
        if instance is None:
            raise Exception(f"player instance is null. path: {path}")

        self._running_instance = instance

        self._get_name = getattr(instance, 'get_name', None)
        self._initialize = getattr(instance, 'initialize', None)
        self._move_next = getattr(instance, 'move_next', None)

        if not all([self._get_name, self._initialize, self._move_next]):
            raise Exception("One or more required methods are missing.")

    def get_name(self):
        if self._get_name:
            return self._get_name()
        return ""

    def initialize(self, my_number, column, row):
        if self._initialize:
            self._initialize(my_number, column, row)
        else:
            raise Exception("The initialize method is undefined.")

    def move_next(self, map, my_position):
        if self._move_next:
            return self._move_next(map, my_position)
        else:
            raise Exception("move_next method is undefined.")
