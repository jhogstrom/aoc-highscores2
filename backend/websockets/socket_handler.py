import boto3
import utils
from boto3.dynamodb.conditions import Key
from botocore.exceptions import ClientError
import json
import datetime

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
        logger.debug(f"Delivered to {id}")
        return None
    except ClientError as e:
        logger.exception(e)
        return id


def save_connection(connection_id: str) -> dict:
    VISITSTABLE.put_item(
        Item={
            'id': "CONNECTION",
            'sk': connection_id
        }
    )
    message = {
        "message": "Connected"
    }
    notify_client(connection_id, payload = json.dumps({"message": "Connection registered"}).encode())
    return { "statusCode": 200, "body": json.dumps(message) }


def delete_connection(connection_id: str) -> dict:
    VISITSTABLE.delete_item(
        Key=Key('id').eq("CONNECTION") & Key('sk').eq(connection_id))
    logger.debug(f"Deleted connection {connection_id}")
    message = {
        "message": "Disonnected"
    }
    return { "statusCode": 200, "body": json.dumps(message) }


def register_interest(connection_id: str, body: dict) -> dict:
    command = body.get("command", "UNKNOWN")
    board_id = body.get("guid", "NO BOARD SET")
    year = body.get("year", "")
    logger.debug(body)

    message = f"Unknown command ´{command}'."
    status_code = 400

    if command.upper() == "REGISTER":
        id = utils.make_subscribe_id(year, board_id)
        sk = connection_id
        ttl = int((datetime.datetime.now() + datetime.timedelta(days=1)).timestamp())
        VISITSTABLE.put_item(
            Item={
                'id': id,
                'sk': sk,
                "ttl": ttl
            }
        )
        message =f"Registered {connection_id} for {board_id}/{year}"
        notify_client(connection_id, payload = json.dumps({"message": message}).encode())

        status_code = 200

    if command.upper() == "UNREGISTER":
        id = utils.make_subscribe_id(year, board_id)
        sk = connection_id
        VISITSTABLE.delete_item(Key={'id': id, 'sk': sk})
        message = f"Unregistered {connection_id} for {board_id}/{year}"
        notify_client(connection_id, payload = json.dumps({"message": message}).encode())
        status_code = 200

    logger.debug(message)
    message = {"message": message}
    return { "statusCode": status_code, "body": json.dumps(message) }


def route_commands(route_key: str, connection_id: str, body):
    logger.debug(f"Client {connection_id}: {route_key}")
    if route_key == "$connect":
        return save_connection(connection_id)

    if route_key == "$disconnect":
        return delete_connection(connection_id)

    if route_key == "$default":
        return register_interest(connection_id, body)


def main(event, context):
    connection_id = event.get("requestContext", {}).get("connectionId")
    route_key = event.get("requestContext", {}).get("routeKey")
    body = json.loads(event.get("body", "{}"))
    # print(body)
    # print(event)
    return route_commands(route_key, connection_id, body)

if __name__ == "__main__":
    route_commands("$default", "foo", {"command": "unregister", "guid": "goo", "year": "2021"})