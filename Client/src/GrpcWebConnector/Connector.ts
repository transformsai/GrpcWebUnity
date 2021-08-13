
export interface Connector {
  Module: UnityPlayer;
  OnHeaders: (channelCallMetadata: string) => void;
  OnServerStreamingResponse: (channelCallMessage: string) => void;
  OnCallCompletion: (channelCallStatusDetailMessage: string) => void;
  OnInstanceRegistered: (instanceKey: number) => void;
}
