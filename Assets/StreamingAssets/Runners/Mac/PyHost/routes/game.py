from fastapi import Request , APIRouter, Depends, Query, HTTPException, BackgroundTasks
from fastapi.responses import PlainTextResponse
from pydantic import BaseModel
from PyHost.services.game_service import GameService
from PyHost.services.player_service import PlayerService
from PyHost.services.player_loader import PlayerLoader

router = APIRouter()

def get_game_service(request: Request) -> GameService:
    game_service_instance = request.app.state.game_service_instance
    if game_service_instance is None:
        raise HTTPException(status_code=500, detail="GameService not initialized")
    return game_service_instance

@router.get("/")
async def get_current_game_set(game_service: GameService = Depends(get_game_service)):
    current_set = game_service.get_current_game_set()
    return current_set

@router.get("/healthy")
async def healthy():
    return PlainTextResponse(content="Healthy", status_code=200)

@router.post("/set")
async def set_game(gameId: str = Query(...), column: int = Query(...), row: int = Query(...), game_service: GameService = Depends(get_game_service)):
    try:
        game_service.init_game(gameId, column, row)
        return PlainTextResponse(content="Game set successfully", status_code=200)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/shutdown")
async def shutdown(background_tasks: BackgroundTasks):
    def stop_app():
        import os
        os._exit(0)
    background_tasks.add_task(stop_app)
    return PlainTextResponse(content="Server is shutting down", status_code=200)

@router.post("/cleanup")
async def cleanup(game_service: GameService = Depends(get_game_service)):
    game_service.clean_up()
    return PlainTextResponse(content="Clean up successful", status_code=200)