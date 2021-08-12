
export interface Connector {
  Module: UnityPlayer;
  OnUnaryResponse: (channelCallMessage: string) => void;
  OnServerStreamingResponse: (channelCallMessage: string) => void;
  OnServerStreamingComplete: (channelCall: string) => void;
  OnCallError: (channelCallError: string) => void;
  OnHeaders: (channelCallMetadata: string) => void;
  OnStatus: (channelCallStatus: string) => void;
  OnInstanceRegistered: (instanceKey: number) => void;
}
