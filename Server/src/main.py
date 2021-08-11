from typing import cast
import os
import asyncio
from grpc import Server, aio

from grpcwebunity.service_pb2_grpc import add_TestServiceServicer_to_server
from server import TestService

HOST = os.getenv("HOST", "0.0.0.0")
PORT = os.getenv("PORT", "8080")

async def serve():
    server = aio.server()
    add_TestServiceServicer_to_server(TestService(), cast(Server, server))
    serve_address = f"{HOST}:{PORT}"
    serving_port = server.add_insecure_port(serve_address)
    print(f"Listening at: {HOST}:{serving_port}")
    await server.start()
    try:
        await server.wait_for_termination()
    except KeyboardInterrupt:
        # Shuts down the server with 0 seconds of grace period. During the
        # grace period, the server won't accept new connections and allow
        # existing RPCs to continue within the grace period.
        await server.stop(0)

asyncio.run(serve())
