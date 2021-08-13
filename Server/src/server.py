import asyncio
from typing import Generator
from grpc import ServicerContext
from grpcwebunity.service_pb2 import Request
from grpcwebunity.service_pb2_grpc import TestServiceServicer
from grpcwebunity.service_pb2 import Request, Response


class TestService(TestServiceServicer):
    def Unary(self, request: Request, context: ServicerContext) -> Response:
        return Response(
            data="Hello Dante!" + request.data
        )

    async def ServerStream(self, request: Request, context: ServicerContext) -> Generator[Response, None, None]:
        for i in range(0, 20):
            yield Response(
                data="request.data" + str(i)
            )
            await asyncio.sleep(3)
