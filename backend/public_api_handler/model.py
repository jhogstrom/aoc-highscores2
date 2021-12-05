from typing import List, Optional
from pydantic import BaseModel, Field
from pydantic.networks import EmailStr


class BoardSpecification(BaseModel):
    boardid: int
    boardname: str
    session_cookie: str
    email: Optional[EmailStr]
