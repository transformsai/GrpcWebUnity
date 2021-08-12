import { encode, decode } from "base64-arraybuffer";
import { RpcMetadata } from "@protobuf-ts/runtime-rpc/build/types/rpc-metadata";

var utf8Encoder = new TextEncoder();
var utf8Decoder = new TextDecoder();
export function toBase64(str: string): string {
  var bytes = utf8Encoder.encode(str);
  return encode(bytes.buffer);
}
export function fromBase64(str: string): string {
  var bytes = decode(str);
  return utf8Decoder.decode(bytes);
}

export const BinarySuffix = "-bin";
export function EncodeMetadata(metadata?: RpcMetadata): string | null {
  if (!metadata) return null;
  var entries = Object.entries(metadata);
  if (entries.length == 0) return null;
  var builder = "";
  for (var entry of entries) {
    var key = entry[0];
    var values: string[];
    if (Array.isArray(entry[1])) values = entry[1];
    else values = [entry[1]];
    for (var value of values) {

      builder += toBase64(key);
      builder += '|';
      if (key.endsWith(BinarySuffix)) builder += value;
      else builder += toBase64(value);
    }
  }
  return builder;
}
export function DecodeMetadata(str?: string): RpcMetadata | undefined {
  if (!str) return undefined;
  var lines = splitLines(str);
  var metadata: RpcMetadata = {};
  for (var line of lines) {
    var [key, value] = line.split("|");
    key = fromBase64(key);
    if(!key.endsWith(BinarySuffix)) value = fromBase64(value);

    var existing = metadata[key];
    if(Array.isArray(existing)) existing.push(value);
    else if(typeof existing == "string") metadata[key] = [existing, value]
    else metadata[key] = value;
  }

  return metadata;
}

const splitLines = function (str: string) {
  return str.split(/\r?\n/);
}