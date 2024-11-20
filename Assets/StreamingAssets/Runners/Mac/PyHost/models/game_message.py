from pydantic import BaseModel
from typing import List

class GameMessage(BaseModel):
    turn: int
    position: int
    map: List[int]
    current: int

    # __iter__를 정의
    def __iter__(self):
        return iter((self.turn, self.position, self.map, self.current))