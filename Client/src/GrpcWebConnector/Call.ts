import { RpcCallShared } from "@protobuf-ts/runtime-rpc/build/types/rpc-call-shared";
import { UnityProtoMessage } from "./UnityProtoMessage";
import { Channel } from "./Channel";


export class Call {
  readonly channel: Channel;
  readonly callobj: RpcCallShared<UnityProtoMessage, UnityProtoMessage>;
  readonly aborter: AbortController;

  constructor(channel: Channel, aborter: AbortController, callobj: RpcCallShared<UnityProtoMessage, UnityProtoMessage>) {
    this.callobj = callobj;
    this.channel = channel;
    this.aborter = aborter;
  }

  cancel() { this.aborter.abort(); }
}
