import { BinaryReadOptions, BinaryWriteOptions, FieldInfo, IBinaryReader, IBinaryWriter, IMessageType, JsonReadOptions, JsonValue, JsonWriteOptions, JsonWriteStringOptions, PartialMessage } from "@protobuf-ts/runtime";

export interface UnityProtoMessage {
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
    fromBinary(data: Uint8Array, _options?: Partial<BinaryReadOptions>): UnityProtoMessage {
      return { bytes: new Uint8Array(data) };
    }
    toBinary(message: UnityProtoMessage, _options?: Partial<BinaryWriteOptions>): Uint8Array {
      return new Uint8Array(message.bytes);
    }
    fromJson(_json: JsonValue, _options?: Partial<JsonReadOptions>): UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
    fromJsonString(_json: string, _options?: Partial<JsonReadOptions>): UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
    toJson(_message: UnityProtoMessage, _options?: Partial<JsonWriteOptions>): JsonValue {
      throw new Error("Method not implemented.");
    }
    toJsonString(_message: UnityProtoMessage, _options?: Partial<JsonWriteStringOptions>): string {
      throw new Error("Method not implemented.");
    }
    clone(message: UnityProtoMessage): UnityProtoMessage {
      return { bytes: new Uint8Array(message.bytes) };
    }
    mergePartial(_target: UnityProtoMessage, _source: PartialMessage<UnityProtoMessage>): void {
      throw new Error("Method not implemented.");
    }
    equals(a: UnityProtoMessage | undefined, b: UnityProtoMessage | undefined): boolean {
      return Boolean(a?.bytes.length === b?.bytes.length && a?.bytes.every((v, i) => v === b?.bytes[i]));
    }
    is(_arg: any, _depth?: number): _arg is UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
    isAssignable(_arg: any, _depth?: number): _arg is UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
    internalJsonRead(_json: JsonValue, _options: JsonReadOptions, _target?: UnityProtoMessage): UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
    internalJsonWrite(_message: UnityProtoMessage, _options: JsonWriteOptions): JsonValue {
      throw new Error("Method not implemented.");
    }
    internalBinaryWrite(_message: UnityProtoMessage, _writer: IBinaryWriter, _options: BinaryWriteOptions): IBinaryWriter {
      throw new Error("Method not implemented.");
    }
    internalBinaryRead(_reader: IBinaryReader, _length: number, _options: BinaryReadOptions, _target?: UnityProtoMessage): UnityProtoMessage {
      throw new Error("Method not implemented.");
    }
  }
  
  export const UnityProtoMessage = new UnityProtoMessageType();
  