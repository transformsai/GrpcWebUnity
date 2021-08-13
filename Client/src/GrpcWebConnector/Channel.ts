import { encode, decode } from "base64-arraybuffer";
import { GrpcStatusCode, GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import { Instance } from "./Instance";
import { UnityProtoMessage } from "./UnityProtoMessage";
import { MethodInfo, RpcError, RpcOptions, RpcMetadata, RpcStatus } from "@protobuf-ts/runtime-rpc";
import { DecodeMetadata, EncodeMetadata, toBase64 } from "./Utils";
import { Call } from "./Call";



type GrpcCodeString = keyof typeof GrpcStatusCode;

function CodeNum(str: String) {
  return GrpcStatusCode[<GrpcCodeString>str];
}

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

  private reportFinishedCall(callKey: number, status: RpcStatus, trailers: RpcMetadata, message?: UnityProtoMessage) {
    let params = [
      this.channelKey,
      callKey,
      CodeNum(status.code),
      toBase64(status.detail),
      toBase64(EncodeMetadata(trailers))
    ];
    if(message) params.push(encode(message.bytes.buffer))
    
    this.instance.unityCaller.OnCallCompletion(
      params.join("|")
    )
  }

  unaryRequest(serviceName: string, methodName: string, headers: string, base64Message: string, deadlineTimestampSecs: number): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.Unary);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
    const aborter = new AbortController();
    let options: RpcOptions = {
      abort: aborter.signal,
      meta: DecodeMetadata(headers),
      timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1000) : undefined
    };
    options = this.transport.mergeOptions(options);
    const call = this.transport.unary(requestMethod, request, options);

    const callObj = new Call(this, aborter, call);
    const callKey = Channel.callCounter++;
    this.callMap.set(callKey, callObj);

    call.headers.then(it => this.instance.unityCaller.OnHeaders(
      [this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join("")
    ))

    call.then(
      it => this.reportFinishedCall(callKey, it.status, it.trailers,it.response),
      it => it instanceof RpcError ?
        this.reportFinishedCall(callKey, {code: it.code, detail:it.message}, it.meta) :
        this.reportFinishedCall(callKey, {code: GrpcStatusCode[GrpcStatusCode.INTERNAL], detail:"Internal error in Channel.ts" + it.message}, it.meta)
      );

    return callKey;
  }



  serverStreamRequest(serviceName: string, methodName: string, headers: string, base64Message: string, deadlineTimestampSecs: number): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.ServerStreaming);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
    const aborter = new AbortController();
    let options: RpcOptions = {
      abort: aborter.signal,
      meta: DecodeMetadata(headers),
      timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1000) : undefined
    };
    options = this.transport.mergeOptions(options);
    const call = this.transport.serverStreaming(requestMethod, request, options);
    const callObj = new Call(this, aborter, call);
    const callKey = Channel.callCounter++;
    this.callMap.set(callKey, callObj);

    call.headers.then(it => this.instance.unityCaller.OnHeaders(
      [this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join("")
    ))

    call.responses.onMessage((message) => {
      const encodedMessage = encode(message.bytes.buffer);
      this.instance.unityCaller.OnServerStreamingResponse(
        [this.channelKey, callKey, encodedMessage].join("|"));
      return;
    });

    call.then(
      it => this.reportFinishedCall(callKey, it.status, it.trailers),
      it => it instanceof RpcError ?
        this.reportFinishedCall(callKey, {code: it.code, detail:it.message}, it.meta) :
        this.reportFinishedCall(callKey, {code: GrpcStatusCode[GrpcStatusCode.INTERNAL], detail:"Internal error in Channel.ts" + it.message}, it.meta)
      );
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

