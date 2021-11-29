import json
import os
import boto3
import botocore
from boto3.dynamodb.conditions import Key
import datetime
from decimal import Decimal
try:
    import envvars
except:
    pass


TIMESTAMPDB_NAME = os.environ.get("TIMESTAMPDB", envvars.TIMESTAMPDB)
dynamodb = boto3.resource('dynamodb')
TABLE = dynamodb.Table(TIMESTAMPDB_NAME)

REGENERATOR_Q_URL = os.environ.get("REGENERATOR_Q", envvars.REGENERATOR_Q)
sqs = boto3.resource('sqs')
REGENERATOR_Q = sqs.Queue(REGENERATOR_Q_URL)



def save_timestamp(id: str, now, previous_timestamp=None):
    params = {
        "Item": {"id": id, "timestamp": now},
        "ReturnValues": "ALL_OLD"
    }

    if previous_timestamp is not None:
        params["ConditionExpression"] = Key("timestamp").eq(previous_timestamp)

    # Try to write an updated timestamp. If the timestamp has changed under our feet
    # indicate by returning false.
    try:
        TABLE.put_item(**params)
        print(f"Timestamp updated for '{id}'")
        return True
    except dynamodb.meta.client.exceptions.ConditionalCheckFailedException:
        print("Someone managed to request a regeneration before us!")
        return False

def request_regeneration(boardguid: str, year: int):
    print(f"Requesting regeneration for [{boardguid}] year {year}")
    REGENERATOR_Q.send_message(
        MessageBody=json.dumps({
            'boardguid': boardguid,
            'year': year
        })
    )


def cooldown_expired(now, timestamp) -> bool:
    MINIMUM_COOL_DOWN = 20
    seconds_since_last_regeneration = now - timestamp
    if seconds_since_last_regeneration > MINIMUM_COOL_DOWN:
        print(f"It's been more than {MINIMUM_COOL_DOWN} seconds since last generation. Let's do it again!")
        return True

    print(f"Hold your horses. Only {int(seconds_since_last_regeneration)} as passed (need to wait {MINIMUM_COOL_DOWN} seconds).")
    return False

def handle_payload(boardguid: str, year: int):
    id = f"{boardguid}|{year}"
    resp = TABLE.query(KeyConditionExpression=Key('id').eq(id))
    items = resp["Items"]
    now = Decimal(datetime.datetime.now().timestamp())

    if len(items) == 0:
        print(f"No timestamp record found for board '{id}'.")
        save_timestamp(id, now)
        request_regeneration(boardguid, year)
        return

    item = items[0]
    last_timestamp = item["timestamp"]
    if cooldown_expired(now, last_timestamp) and save_timestamp(id, now, last_timestamp):
        request_regeneration(boardguid, year)


def main(event, context):
    for record in event["Records"]:
        payload = json.loads(record.get("body", {}))
        handle_payload(payload.get("boardguid"), payload.get("year"))


if __name__ == "__main__":
    handle_payload("abc", 1010)
    exit()
