import shutil, os
from pathlib import Path
from os.path import normpath, join, dirname
from grpc_tools import protoc

PROTO_NAMESPACE = "grpcwebunity"
PARENT_PATH = normpath(join(dirname(__file__), ".."))
PROTO_PATH = normpath(join(PARENT_PATH, "..", "Proto"))
PROTO_NAMESPACE_PATH = join(PROTO_PATH, PROTO_NAMESPACE)
OUTPUT_PATH = join(PARENT_PATH, "src")

def generate_stubs():
    protoc.main([
        "", # Required
        f"--proto_path={PROTO_PATH}",
        f"--python_out={OUTPUT_PATH}",
        f"--mypy_out=readable_stubs:{OUTPUT_PATH}",
        f"--grpc_python_out={OUTPUT_PATH}",
        f"--mypy_grpc_out=readable_stubs:{OUTPUT_PATH}",
        join(PROTO_NAMESPACE_PATH, "service.proto"),
    ])
    Path(join(OUTPUT_PATH, PROTO_NAMESPACE, '__init__.py')).touch()

if __name__ == "__main__":
    generate_stubs()
