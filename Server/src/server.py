import asyncio
from typing import Generator
from grpc import ServicerContext
from grpcwebunity.service_pb2 import Request
from grpcwebunity.service_pb2_grpc import TestServiceServicer
from grpcwebunity.service_pb2 import Request, Response


class TestService(TestServiceServicer):
    def Unary(self, request: Request, context: ServicerContext) -> Response:
        print("got Unary: " + request.data)
        return Response(
            data=f"Server Received: {request.data} \nUnary Response: Hello Dante!"
        )

    async def ServerStream(self, request: Request, context: ServicerContext) -> Generator[Response, None, None]:
        print("got Stream: " + request.data)
        max = 20
        for i in range(0, max):
            yield Response(
                data=f"Server Received: {request.data} \nStream Response: {str(i+1)} out of {max}"
            )
            await asyncio.sleep(3)
