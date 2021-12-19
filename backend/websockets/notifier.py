from typing import List
import boto3
import utils
import json
from botocore.exceptions import ClientError
import concurrent.futures
from boto3.dynamodb.conditions import Key
from boto3.dynamodb.conditions import Attr
import os

logger = utils.setup_logger("WSDemo")
config = utils.get_config()
endpoint = config['WEBSOCKET_API_ENDPOINT']
client = boto3.client('apigatewaymanagementapi', endpoint_url=endpoint)
VISITSTABLE = boto3.resource('dynamodb').Table(config["CONNECTION_TABLE"])
logger.info(f"Using table '{VISITSTABLE.name}'.")
logger.info(f"Sending to endpoint '{endpoint}'.")


def notify_client(id: str, payload: str):
    logger.debug(f"Notifying {id}")
    try:
        client.post_to_connection(ConnectionId=id, Data=payload)
        # logger.debug(f"Delivered to {id}")
        return None
    except ClientError as e:
        logger.exception(e)
        return id

def send_notification(year: str, board_guid: str, subscribers: list, executor) -> List[str]:
    res = []
    body = {
        "event": "update",
        "year": year,
        "boardguid": board_guid
    }
    payload = json.dumps(body).encode()
    return [executor.submit(notify_client, s["sk"], payload) for s in subscribers]

def delete_clients(year, board_guid, deleted_clients: List[str]):
    if not deleted_clients:
        return
    logger.info(f"The following clients were not found: {deleted_clients}")
    with VISITSTABLE.batch_writer() as batch:
        for id in deleted_clients:
            batch.delete_item(
                Key={
                    'id': utils.make_subscribe_id(year, board_guid),
                    'sk': id
                })
            batch.delete_item(Key={
                'id': "CONNECTION",
                'sk': id
            })


def notify_subscribers(year: str, board_guid: str):
    logger.debug(f"Changed data: {year} - '{board_guid}'")
    with concurrent.futures.ThreadPoolExecutor(max_workers=30) as executor:
        id = utils.make_subscribe_id(year, board_guid)
        resp = VISITSTABLE.query(
            KeyConditionExpression=Key("id").eq(id),
            ProjectionExpression="sk"
        )
        logger.debug(f"Fetched {len(resp['Items'])} clients for '{id}'.")
        deleted_clients = send_notification(year, board_guid, resp["Items"], executor)
        while resp.get("LastEvaluatedKey") is not None:
            resp = VISITSTABLE.scan(ExclusiveStartKey=resp["LastEvaluatedKey"])
            logger.debug(f"Fetched {len(resp['Items'])} clients.")
            deleted = send_notification(year, board_guid, resp["Items"], executor)
            deleted_clients.extend(deleted)

    delete_clients(year, board_guid, list(
        filter(
            lambda _: _ is not None,
            [f.result() for f in deleted_clients]
        )
    ))

def handle_key(key: str):
    logger.debug(f"Key: {key}")
    year, filename = key.split("/")
    board_guid = filename.replace(".json", "")
    notify_subscribers(year, board_guid)


def main(event, context):
    # print(event)
    for record in event.get("Records", []):
        handle_key(record["s3"]["object"]["key"])

if __name__ == "__main__":
    handle_key("2021/foobar.json")