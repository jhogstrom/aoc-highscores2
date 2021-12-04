from typing import List, Optional
from pydantic import BaseModel, Field


class BoardSpecification(BaseModel):
    boardid: int
    boardname: str
    session_cookie: str
    password: str
    ownerid: str
