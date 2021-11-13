from typing import List, Optional
from pydantic import BaseModel, Field


class BoardBase(BaseModel):
    owner: str
    name: str

class BoardInfo(BoardBase):
    boardguid: str
    aocid: str

class NameMap(BaseModel):
    from_id: int
    to_name: str

class BoardInfoDetails(BoardInfo):
    namemaps: List[NameMap]
    years: List[int]

