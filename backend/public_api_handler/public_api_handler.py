from json.decoder import JSONDecodeError
from boto3 import session
from fastapi import FastAPI, Request, Response, status
from fastapi.middleware.cors import CORSMiddleware
from mangum import Mangum
import logging
import sys
import os
import boto3
import json
import uuid
import datetime
import requests

from model import BoardSpecification
try:
    import envvars
except:
    pass

SES_BAN_LIFTED = False

CONFIGDB_NAME = os.environ.get("CONFIGDB", envvars.CONFIGDB)
dynamodb = boto3.resource('dynamodb')
TABLE = dynamodb.Table(CONFIGDB_NAME)


REFRESHQ_URL = os.environ.get("REFRESHQ", envvars.REFRESHQ)
sqs = boto3.resource('sqs')
REFRESHQ = sqs.Queue(REFRESHQ_URL)

MAILER = client = boto3.client('ses')
SENDER_EMAIL = 'jspr.hgstrm+aoc@gmail.com'

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


def validate_board(boardid, session_cookie):
    url = f"https://adventofcode.com/2021/leaderboard/private/view/{boardid}.json"
    logger.debug(f"Requesting {url}")

    cookies = {"session": session_cookie}
    response = requests.get(url, cookies=cookies)
    logger.debug(f"Validation of {boardid} -> {response.status_code}")
    try:
        res = json.loads(response.content)
        logger.debug("Response is valid json")
        return response.status_code == 200
    except JSONDecodeError as e:
        logger.debug("Response not json")
        return False


def send_email(*, to_address, subject, html_content):
    resp = MAILER.send_email(
        Source=SENDER_EMAIL,
        Destination={
            "ToAddresses": [to_address],
            "BccAddresses": [SENDER_EMAIL]},
        Message={
            "Subject": { "Data": subject },
            "Body": { "Html": {"Data": html_content } },
        },
        ReplyToAddresses=[SENDER_EMAIL],
    )
    print(f"Message sent to {to_address} ({resp['MessageId']}).")


def board_generated_content(board_name: str, guid: str):
    return f"""
    <body>
    <html>
    <h1>The board '{board_name}' has been generated!</h1>

    <p>
    You can access your board by visiting
    <a href="https://aoc.lillfiluren.se/guid={guid}">https://aoc.lillfiluren.se/guid={guid}</a>.
    </p>

    <p>
    Make a note of your board guid (<b>{guid}</b>) - for instance by bookmarking the url above.
    </p>

    <p>
    Feel free to share the url to all members on your board as well as friends, family and co-workers.
    </p>

    <p>
    <i>//Jesper and Jonas, maintainers of AocLeaderBoard++</i>
    </p>
    <p>
    PS: Good luck on <a href="http://adventofcode.com">Advent of Code</a>!
    </p>
    <p>
    PPS: If you did not expect this email, do not worry.
    Nothing has been compromised, but someone signed up for a board in your name.
    Don't click on any links, simply delete the email. You may want to look up 'advent of code' - it is a really neat
    programming competition that runs every December.
    </p>

    </html>
    </body>
    """

@app.post("/createboard", status_code=status.HTTP_201_CREATED)
def create_board(board: BoardSpecification, response: Response):
    if not validate_board(board.boardid, board.session_cookie):
        response.status_code = status.HTTP_412_PRECONDITION_FAILED
        return {
            "message": f"Board could not be verified. No entry created.",
        }
    boardguid = str(uuid.uuid4())
    item = {
        'id': boardguid,
        'sk': f"BOARDINFO|{board.boardid}",
        'name': board.boardname,
        'email': board.email,
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
    if board.email and (SES_BAN_LIFTED or board.email == SENDER_EMAIL):
        send_email(
            to_address=board.email,
            subject="Your enhanced AOC leader board has been created",
            html_content=board_generated_content(board_name=board.boardname, guid=boardguid))
    return {
        "message": f"Created board {board.boardname}/{board.boardid}",
        "guid": boardguid
    }


if __name__ == "__main__":
    boardid = envvars.BOARDID
    session = envvars.SESSION_COOKIE
    x = create_board(BoardSpecification(
        boardid=boardid,
        boardname="XXXDELETE",
        session_cookie=envvars.SESSION_COOKIE,
        email=""),
        Response())
    # print(x)
    # x = validate_board(boardid, session)
    # print(f"Board {boardid} valid: {x}")
    # send_email(
    #     to_address=envvars.TEST_RECEIVER,
    #     subject="Your enhanced AOC leader board has been created",
    #     html_content=board_generated_content("Jesper's test board", "some_random_guid")
    # )
