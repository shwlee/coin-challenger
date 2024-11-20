from fastapi import Request, APIRouter, Depends, Form, Path, Body, HTTPException
from fastapi.responses import JSONResponse
from fastapi.responses import PlainTextResponse
from pydantic import BaseModel
from typing import Optional
import asyncio
import os
from PyHost.models.game_message import GameMessage
from PyHost.services.game_service import GameService
from PyHost.services.player_service import PlayerService
from PyHost.services.player_loader import PlayerLoader

router = APIRouter()

def get_game_service(request: Request) -> GameService:
    game_service_instance = request.app.state.game_service_instance
    if game_service_instance is None:
        raise HTTPException(status_code=500, detail="GameService not initialized")
    return game_service_instance

@router.post("/load")
async def load_player(
    position: int = Form(...),
    filePath: str = Form(...),
    game_service: GameService = Depends(get_game_service)
):
    try:
        await game_service.load_player(position, filePath)
        return PlainTextResponse(content="Player loaded successfully", status_code=200)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/init")
async def initialize_player(
    position: int = Form(...),
    column: int = Form(...),
    row: int = Form(...),
    game_service: GameService = Depends(get_game_service)
):
    try:
        game_service.initialize_player(position, column, row)
        return PlainTextResponse(content="Player initialized successfully", status_code=200)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/name/{position}")
async def get_player_name(
    position: int = Path(...),
    game_service: GameService = Depends(get_game_service)
):
    try:
        name = game_service.get_player_name(position)
        return PlainTextResponse(content=name, status_code=200)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/movenext")
async def move_next(
    message: GameMessage = Body(...),
    game_service: GameService = Depends(get_game_service)
):
    try:
        direction = await game_service.move_next(message)
        return JSONResponse(content=direction, status_code=200)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))