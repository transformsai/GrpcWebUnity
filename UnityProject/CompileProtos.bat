rmdir /s /q "./Assets/Scripts/Generated"
mkdir "./Assets/Scripts/Generated"
mkdir "./Assets/Scripts/Generated/Descriptors"

"../Proto/ProtoCompiler/windows_x64/protoc.exe" -I ../Proto --include_source_info --csharp_out=./Assets/Scripts/Generated --descriptor_set_out=./Assets/Scripts/Generated/descriptors.pb2 --grpc_out=./Assets/Scripts/Generated --plugin=protoc-gen-grpc=../Proto/ProtoCompiler/windows_x64/grpc_csharp_plugin.exe ../Proto/grpcwebunity/*.proto
