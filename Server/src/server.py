import asyncio
from typing import Generator
from grpc import ServicerContext
from grpcwebunity.service_pb2 import Request
from grpcwebunity.service_pb2_grpc import TestServiceServicer
from grpcwebunity.service_pb2 import Request, Response

class TestService(TestServiceServicer):
    def Unary(self, request: Request, context: ServicerContext) -> Response:
        return Response(
            data="Hello Dante!"
        )

    async def ServerStream(self, request: Request, context: ServicerContext) -> Generator[Response, None, None]:
        while True:
            yield Response(
                data="Hello Dante!"
            )
            await asyncio.sleep(3)
