import sys
import os
import logging
from typing import Dict
import dotenv


def setup_logger(name, level: int = logging.DEBUG) -> logging.Logger:
    logger = logging.getLogger(name)
    logger.setLevel(level)
    handler = logging.StreamHandler(sys.stderr)
    handler.setLevel(level)
    formatter = logging.Formatter('%(asctime)s | %(levelname)s | %(name)s | %(message)s')
    handler.setFormatter(formatter)
    logger.addHandler(handler)
    return logger


def get_config(envfile: str = None) -> Dict[str, str]:
    filename = envfile or ".env"
    curdir = os.path.dirname(os.path.abspath(__file__))
    filename = os.path.join(curdir, filename)
    return {
        **dotenv.dotenv_values(filename),
        **os.environ
    }

def make_subscribe_id(year, board_guid) -> str:
    return f"SUBSCRIBE#{board_guid}#{year}"