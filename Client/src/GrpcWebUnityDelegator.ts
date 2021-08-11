import Base64ArrayBuffer from "base64-arraybuffer";
import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport"
import { BinaryReadOptions, BinaryWriteOptions, FieldInfo, IBinaryReader, IBinaryWriter, IMessageType, JsonReadOptions, JsonValue, JsonWriteOptions, JsonWriteStringOptions, PartialMessage } from "@protobuf-ts/runtime";

import { MethodInfo } from "@protobuf-ts/runtime-rpc";
import { RpcCallShared } from "@protobuf-ts/runtime-rpc/build/types/rpc-call-shared";

interface UnityProtoMessage {
  bytes: Uint8Array;
}

class UnityProtoMessageType implements IMessageType<UnityProtoMessage> {
  typeName: string = "UnityMessage";
  fields: readonly FieldInfo[] = [];
  options: { [extensionName: string]: JsonValue; } = {};

  create(value?: PartialMessage<UnityProtoMessage>): UnityProtoMessage {
    if (!value) return { bytes: new Uint8Array(0) };
    throw new Error("Method not implemented.");
  }
  fromBinary(data: Uint8Array, options?: Partial<BinaryReadOptions>): UnityProtoMessage {
    return { bytes: new Uint8Array(data) };
  }
  toBinary(message: UnityProtoMessage, options?: Partial<BinaryWriteOptions>): Uint8Array {
    return new Uint8Array(message.bytes);
  }
  fromJson(json: JsonValue, options?: Partial<JsonReadOptions>): UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
  fromJsonString(json: string, options?: Partial<JsonReadOptions>): UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
  toJson(message: UnityProtoMessage, options?: Partial<JsonWriteOptions>): JsonValue {
    throw new Error("Method not implemented.");
  }
  toJsonString(message: UnityProtoMessage, options?: Partial<JsonWriteStringOptions>): string {
    throw new Error("Method not implemented.");
  }
  clone(message: UnityProtoMessage): UnityProtoMessage {
    return { bytes: new Uint8Array(message.bytes) };
  }
  mergePartial(target: UnityProtoMessage, source: PartialMessage<UnityProtoMessage>): void {
    throw new Error("Method not implemented.");
  }
  equals(a: UnityProtoMessage | undefined, b: UnityProtoMessage | undefined): boolean {
    return Boolean(a?.bytes.length === b?.bytes.length && a?.bytes.every((v, i) => v === b?.bytes[i]));
  }
  is(arg: any, depth?: number): arg is UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
  isAssignable(arg: any, depth?: number): arg is UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
  internalJsonRead(json: JsonValue, options: JsonReadOptions, target?: UnityProtoMessage): UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
  internalJsonWrite(message: UnityProtoMessage, options: JsonWriteOptions): JsonValue {
    throw new Error("Method not implemented.");
  }
  internalBinaryWrite(message: UnityProtoMessage, writer: IBinaryWriter, options: BinaryWriteOptions): IBinaryWriter {
    throw new Error("Method not implemented.");
  }
  internalBinaryRead(reader: IBinaryReader, length: number, options: BinaryReadOptions, target?: UnityProtoMessage): UnityProtoMessage {
    throw new Error("Method not implemented.");
  }
}

const UnityProtoMessage = new UnityProtoMessageType();

function lastElement<T>(arr: Array<T>): T|null {
  return arr.length
    ? arr[arr.length - 1]
    : null;
}

enum GrpcRequestType {
  Unary = 0,
  ServerStreaming = 1,
  ClientStreaming = 2,
  Duplex = 3,
}

function makeUnityMethodInfo(serviceName: string, methodName: string, requestType: GrpcRequestType): MethodInfo<UnityProtoMessage, UnityProtoMessage> {
  return  {
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

function formatUnityCallerMessage(...params: any[]) {
  return params.join("|");
}

class GrpcDelegatorChannel {
  static callCounter: number = 0;

  channelKey: number;
  transport: GrpcWebFetchTransport;
  instance: UnityWebGrpcInstance;

  callMap = new Map<number, RpcCallShared<UnityProtoMessage, UnityProtoMessage>>();

  constructor(channelKey: number, address: string, instance: UnityWebGrpcInstance) {
    this.channelKey = channelKey;
    this.transport = new GrpcWebFetchTransport({ baseUrl: address });
    this.instance = instance;
  }

  unaryRequest(serviceName: string, methodName: string, base64Message: string): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.Unary);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(Base64ArrayBuffer.decode(base64Message)));
    const call = this.transport.unary(requestMethod, request, {});
    const callKey = GrpcDelegatorChannel.callCounter++;
    this.callMap.set(callKey, call);
    call
      .then((value) => {
        const encodedMessage = Base64ArrayBuffer.encode(value.response.bytes.buffer);
        this.instance.unityCaller.OnChannelUnaryResponse(
          formatUnityCallerMessage(
            this.channelKey,
            callKey,
            encodedMessage,
          ));
      });
    return callKey;
  }

  serverStreamRequest(serviceName: string, methodName: string, base64Message: string): number {
    const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.ServerStreaming);
    const request = UnityProtoMessage.fromBinary(new Uint8Array(Base64ArrayBuffer.decode(base64Message)));
    const serverStreamingCall = this.transport.serverStreaming(requestMethod, request, {});
    const callKey = GrpcDelegatorChannel.callCounter++;
    this.callMap.set(callKey, serverStreamingCall);
    const { responses } = serverStreamingCall;
    responses.onNext((message, error, isComplete) => {
      if (message) {
        const encodedMessage = Base64ArrayBuffer.encode(message.bytes.buffer);
        this.instance.unityCaller.OnChannelServerStreamingResponse(
          formatUnityCallerMessage(
            this.channelKey,
            callKey,
            encodedMessage,
          )
        );
      } else if (error) {
        this.instance.unityCaller.OnChannelCallError(
          formatUnityCallerMessage(
            this.channelKey,
            callKey,
            error.message,
          )
        );
      } else if (isComplete) {
        this.instance.unityCaller.OnChannelServerStreamingComplete(
          formatUnityCallerMessage(
            this.channelKey,
            callKey,
          )
        );
      }
    });
    return callKey;
  }

  // TODO: Implement request cancellation
  cancelRequest(callKey: number) {
    const call = this.findCall(callKey);
  }

  findCall(callKey: number): RpcCallShared<UnityProtoMessage, UnityProtoMessage> {
    const call = this.callMap.get(callKey);
    if (!call) throw new Error(`Invalid callKey: ${callKey}`);
    return call;
  }
}

class UnityWebGrpcInstance {
  readonly unityCaller: UnityCaller;
  readonly instanceKey: number;
  readonly channelMap = new Map<number, GrpcDelegatorChannel>();

  private static channelCounter: number = 0;

  constructor(unityCaller: UnityCaller, instanceKey: number) {
    this.unityCaller = unityCaller;
    this.instanceKey = instanceKey;
  }

  makeChannel(address: string): number {
    const channelKey = UnityWebGrpcInstance.channelCounter++;
    const channel = new GrpcDelegatorChannel(channelKey, address, this);
    this.channelMap.set(channelKey, channel);
    return channelKey;
  }

  findChannel(channelKey: number): GrpcDelegatorChannel {
    const channel = this.channelMap.get(channelKey);
    if (!channel) throw new Error(`Invalid channelKey: ${channelKey}`);
    return channel;
  }
}

interface UnityCaller {
  Module: UnityInstance;
  OnChannelUnaryResponse: (channelCallMessage: string) => void;
  OnChannelServerStreamingResponse: (channelCallMessage: string) => void;
  OnChannelServerStreamingComplete: (channelCall: string) => void;
  OnChannelCallError: (channelCallError: string) => void;
  OnInstanceRegistered: (instanceKey: string) => void;
  OnInstanceRegistrationFailure: (errorString: string) => void;
}

export default class GrpcWebUnityDelegator {
  private static instanceCounter: number = 0;

  // private static callCounter: number = 0;

  private instanceMap = new Map<number, UnityWebGrpcInstance>();

  constructor() {}

  registerUnityInstance(unityCaller: UnityCaller) {
    GrpcWebUnityDelegator.instanceCounter++;
    const instance = new UnityWebGrpcInstance(unityCaller, GrpcWebUnityDelegator.instanceCounter);
    this.instanceMap.set(instance.instanceKey, instance);
    return instance.instanceKey;
  }

  registerChannel(instanceKey: number, address: string): number {
    const instance = this.findInstance(instanceKey);
    return instance.makeChannel(address);
  }

  channelUnaryRequest(
    instanceKey: number,
    channelKey: number,
    serviceName: string,
    methodName: string,
    base64Message: string,
  ): number {
    const instance = this.findInstance(instanceKey);
    const channel = instance.findChannel(channelKey);
    return channel.unaryRequest(serviceName, methodName, base64Message);
  }

  channelServerStreamingRequest(
    instanceKey: number,
    channelKey: number,
    serviceName: string,
    methodName: string,
    base64Message: string,
  ): number {
    const instance = this.findInstance(instanceKey);
    const channel = instance.findChannel(channelKey);
    return channel.serverStreamRequest(serviceName, methodName, base64Message);
  }

  findInstance(instanceKey: number): UnityWebGrpcInstance {
    const instance = this.instanceMap.get(instanceKey);
    if (!instance) throw new Error(`Invalid instanceKey: ${instanceKey}`);
    return instance;
  }
}
