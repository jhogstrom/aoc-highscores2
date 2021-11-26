from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from mangum import Mangum
import logging
import sys
import os
import boto3
import json
try:
    import envvars
except:
    pass

REFRESHQ_URL = os.environ.get("REFRESHQ", envvars.REFRESHQ)
# x = os.environ.get("AWS_PROFILE")
# print(x)
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


if __name__ == "__main__":
    request_refresh("abc", 2000)
    print("foo")