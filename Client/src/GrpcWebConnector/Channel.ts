import { encode, decode } from "base64-arraybuffer";
import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import { Instance } from "./Instance";
import { UnityProtoMessage } from "./UnityProtoMessage";
import { MethodInfo, RpcError } from "@protobuf-ts/runtime-rpc";
import { DecodeMetadata, EncodeMetadata, toBase64 } from "./Utils";
import { Call } from "./Call";

export class Channel {
  static callCounter: number = 0;

  channelKey: number;
  transport: GrpcWebFetchTransport;
  instance: Instance;

  callMap = new Map<number, Call>();

  constructor(channelKey: number, address: string, instance: Instance) {
    this.channelKey = channelKey;
    this.transport = new GrpcWebFetchTransport({ baseUrl: address });
    this.instance = instance;
  }

  unaryRequest(serviceName: string, methodName: string, headers: string, base64Message: string, deadlineTimestampSecs: number): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.Unary);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
    const aborter = new AbortController();
    const call = this.transport.unary(requestMethod, request, {
      abort: aborter.signal,
      meta: DecodeMetadata(headers),
      timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1000) : undefined
    });

    const callObj = new Call(this, aborter, call);
    const callKey = Channel.callCounter++;
    this.callMap.set(callKey, callObj);

    call.headers.then(it => this.instance.unityCaller.OnHeaders(
      [this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join()
    ))

    call.status.then(it => this.instance.unityCaller.OnStatus(
      [this.channelKey, "|", callKey, "|", it.code, "\n", it.detail].join()
    ), (err: RpcError) => this.instance.unityCaller.OnStatus(
      [this.channelKey, "|", callKey, "|", err.code, "\n", err.message].join()
    ));

    call.response.then((value) =>
      this.instance.unityCaller.OnUnaryResponse(
        [this.channelKey, callKey, encode(value.bytes.buffer)].join("|"))
      , error => this.instance.unityCaller.OnCallError(
        [this.channelKey, callKey, toBase64("" + error)].join("|")));


    return callKey;
  }

  serverStreamRequest(serviceName: string, methodName: string, headers: string, base64Message: string, deadlineTimestampSecs: number): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.ServerStreaming);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
    const aborter = new AbortController();
    const call = this.transport.serverStreaming(requestMethod, request, {
      abort: aborter.signal,
      meta: DecodeMetadata(headers),
      timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1000) : undefined
    });
    const callObj = new Call(this, aborter, call);
    const callKey = Channel.callCounter++;
    this.callMap.set(callKey, callObj);
    
    call.headers.then(it => this.instance.unityCaller.OnHeaders(
      [this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join()
    ))

    call.status.then(it => this.instance.unityCaller.OnStatus(
      [this.channelKey, "|", callKey, "|", it.code, "\n", it.detail].join()
    ), (err: RpcError) => this.instance.unityCaller.OnStatus(
      [this.channelKey, "|", callKey, "|", err.code, "\n", err.message].join()
    ));

    call.responses.onNext((message, error, isComplete) => {
      if (message) {
        const encodedMessage = encode(message.bytes.buffer);
        this.instance.unityCaller.OnServerStreamingResponse(
          [this.channelKey, callKey, encodedMessage].join("|"));
        return;
      }

      if (error) {
        const encodedErrorMessage = toBase64(error.message);
        this.instance.unityCaller.OnCallError(
          [this.channelKey, callKey, encodedErrorMessage].join("|"));
        return
      }

      if (isComplete) {
        this.instance.unityCaller.OnServerStreamingComplete(
          [this.channelKey, callKey].join("|"));
        return
      }
    });
    return callKey;
  }

  cancelRequest(callKey: number) {
    const call = this.findCall(callKey);
    call.cancel();
  }

  findCall(callKey: number): Call {
    const call = this.callMap.get(callKey);
    if (!call)
      throw new Error(`Invalid callKey: ${callKey}`);
    return call;
  }
}

export enum GrpcRequestType {
  Unary = 0,
  ServerStreaming = 1,
  ClientStreaming = 2,
  Duplex = 3,
}

export function makeUnityMethodInfo(serviceName: string, methodName: string, requestType: GrpcRequestType):
  MethodInfo<UnityProtoMessage, UnityProtoMessage> {

  function lastElement<T>(arr: Array<T>) {
    return arr.length ? arr[arr.length - 1] : null;
  }

  return {
    service: {
      methods: [],
      options: {},
      typeName: serviceName,
    },
    name: methodName,
    localName: lastElement(methodName.split("."))!,
    idempotency: undefined,
    serverStreaming: [GrpcRequestType.ServerStreaming, GrpcRequestType.Duplex].includes(requestType),
    clientStreaming: [GrpcRequestType.ClientStreaming, GrpcRequestType.Duplex].includes(requestType),
    options: {},
    I: UnityProtoMessage,
    O: UnityProtoMessage,
  };
}

