from pydantic import BaseModel

class GameSet(BaseModel):
    column: int
    row: int