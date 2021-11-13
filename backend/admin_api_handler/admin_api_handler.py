from typing import List
import uuid
from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from mangum import Mangum
import logging
import sys
import os
import boto3
from models import BoardBase, BoardInfo, BoardInfoDetails, NameMap
try:
    import envvars
except:
    pass


CONFIGDB_NAME = os.environ.get("CONFIGDB", envvars.CONFIGDB)
dynamodb = boto3.resource('dynamodb')
TABLE = dynamodb.Table(CONFIGDB_NAME)


def setup_logger(name, level: int = logging.DEBUG) -> logging.Logger:
    logger = logging.getLogger(name)
    logger.setLevel(level)
    handler = logging.StreamHandler(sys.stderr)
    handler.setLevel(level)
    formatter = logging.Formatter('%(asctime)s | %(levelname)s | %(name)s | %(message)s')
    handler.setFormatter(formatter)
    logger.addHandler(handler)
    return logger

app = FastAPI()
handler = Mangum(app)
logger = setup_logger("linkedin")

origins = [
    "*",
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.middleware("http")
async def log_invocation(request: Request, call_next):
    logger.info(f"[{request.method}] {request.url.path}")
    if request.path_params:
        logger.info(f"* Path params: {request.path_params}")
    if request.query_params:
        logger.info(f"* Query params: {request.query_params}")
    logger.info(f"Root path: {request.scope.get('root_path')}")

    # url_list = [
    #     {"path": route.path, "name": route.name} for route in request.app.routes
    # ]
    # logoutput = [f"{endpoint['name']} -> {endpoint['path']}" for endpoint in url_list]
    # logger.debug("\n".join(logoutput))

    response = await call_next(request)

    # response.headers["X-Process-Time"] = str(process_time)
    return response


def make_board(data: dict, t) -> BoardInfo:
    sk = data["sk"].split("|")
    return t(
        boardguid = data["id"],
        aocid = sk[1],
        owner = data.get("owner", "<none>"),
        name = data.get("name", f"<board {sk[1]}")
    )

def make_year(data: dict) -> int:
    sk = data["sk"].split("|")
    return int(sk[1])


def make_namemap(data: dict) -> NameMap:
    sk = data["sk"].split("|")
    return NameMap(
        from_id = int(sk[1]),
        to_name = data["to_name"]
    )


def make_session(data: dict) -> str:
    sk = data["sk"].split("|")
    return sk[1]


@app.get("/boards", response_model=List[BoardInfo])
def get_boards():
    resp = TABLE.scan()
    items = resp["Items"]
    print(items)
    result = [make_board(_, BoardInfo) for _ in items]
    return result

@app.post("/boards/{aocid}")
def create_board(aocid: str, board: BoardBase):
    item = {
        "id": uuid.uuid4(),
        "sk": f"BOARDINFO|{aocid}",
        "owner": board.owner,
        "name": board.name
        }
    TABLE.put_item(Item=item)
    return {"message": f"Created {board.name} [{aocid}] with id {item.id}."}

@app.get("/board/{id}")
def get_board_info(id: str):
    resp = TABLE.scan()
    items = resp["Items"]
    namemaps = []
    years = []
    board = None
    session = None
    for item in items:
        sk = item["sk"]
        if "NAMEMAP" in sk:
            namemaps.append(make_namemap(item))
        elif "YEAR" in sk:
            years.append(make_year(item))
        elif "BOARDINFO" in sk:
            board = make_board(item, BoardInfoDetails)
        elif "SESSION" in sk:
            session = make_session(item)

    board.namemaps = namemaps
    board.years = years
    board.session = session
    return board








if __name__ == "__main__":
    print("foo")