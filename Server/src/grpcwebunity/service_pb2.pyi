"""
@generated by mypy-protobuf.  Do not edit manually!
isort:skip_file
"""
from builtins import (
    int,
)

from google.protobuf.descriptor import (
    Descriptor,
    FileDescriptor,
)

from google.protobuf.message import (
    Message,
)

from typing import (
    Text,
)

from typing_extensions import (
    Literal,
)


DESCRIPTOR: FileDescriptor = ...

class Request(Message):
    DESCRIPTOR: Descriptor = ...
    DATA_FIELD_NUMBER: int
    data: Text = ...
    def __init__(self,
        *,
        data : Text = ...,
        ) -> None: ...
    def ClearField(self, field_name: Literal[u"data",b"data"]) -> None: ...

class Response(Message):
    DESCRIPTOR: Descriptor = ...
    DATA_FIELD_NUMBER: int
    data: Text = ...
    def __init__(self,
        *,
        data : Text = ...,
        ) -> None: ...
    def ClearField(self, field_name: Literal[u"data",b"data"]) -> None: ...