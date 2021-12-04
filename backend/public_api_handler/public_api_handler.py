from boto3 import session
from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from mangum import Mangum
import logging
import sys
import os
import boto3
import json
import uuid
import datetime
import hashlib

from model import BoardSpecification
try:
    import envvars
except:
    pass

CONFIGDB_NAME = os.environ.get("CONFIGDB", envvars.CONFIGDB)
dynamodb = boto3.resource('dynamodb')
TABLE = dynamodb.Table(CONFIGDB_NAME)


REFRESHQ_URL = os.environ.get("REFRESHQ", envvars.REFRESHQ)
sqs = boto3.resource('sqs')
REFRESHQ = sqs.Queue(REFRESHQ_URL)

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

@app.get("/refresh/{year}/{boardguid}")
def request_refresh(year: int, boardguid: str):
    try:
        REFRESHQ.send_message(
            MessageBody=json.dumps({
                'boardguid': boardguid,
                'year': year
            })
        )
    except Exception as e:
        return {"message": str(e)}

    return {"message": "Request refresh sent for the board"}


@app.post("/createboard")
def create_board(board: BoardSpecification):
    password = f"AoCRules{board.password}Yehaa"
    password = hashlib.md5(password.encode()).hexdigest()
    boardguid = str(uuid.uuid4())
    item = {
        'id': boardguid,
        'sk': f"BOARDINFO|{board.boardid}",
        'name': board.boardname,
        'password': password,
        'ownerid': board.ownerid,
    }
    TABLE.put_item(Item=item)
    item = {
        'id': boardguid,
        'sk': f"SESSION|{board.session_cookie}",
        'added': datetime.datetime.now().strftime("%Y-%m-%d")
    }
    TABLE.put_item(Item=item)
    year = datetime.datetime.now().year
    if datetime.datetime.now().month < 12:
        year -= 1
    request_refresh(year, boardguid)
    return {
        "message": f"Created board {board.boardname}/{board.boardid}",
        "guid": boardguid
    }


if __name__ == "__main__":
    x = create_board(BoardSpecification(
        boardid=34481,
        boardname="XXXDELETE",
        session_cookie="53616c7465645f5fda698469a336952935bdeeccf99602fe8b841a3a35120a51cb37edc129db7f70afcb9759c945cf65"))
    print(x)