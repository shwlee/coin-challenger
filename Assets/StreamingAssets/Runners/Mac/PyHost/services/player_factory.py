import importlib
import types

class PlayerFactory:
    @staticmethod
    def load_code_module(file_path):
        with open(file_path, 'r',  encoding='utf-8') as file:
            code = file.read()

        module_name = "player_module"
        module = types.ModuleType(module_name)

        try:
            exec(code, module.__dict__)

            player = getattr(module, "Player", None)
            if player is None:
                raise Exception("Can't load Player class")

            instance = player()
            return player, instance

        except Exception as e:
            print(f"Error: {e}")
            return None, None
