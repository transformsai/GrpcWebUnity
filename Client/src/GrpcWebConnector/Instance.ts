import { Channel } from "./Channel";
import { Connector } from "./Connector";

export class Instance {
  readonly unityCaller: Connector;
  readonly instanceKey: number;
  readonly channelMap = new Map<number, Channel>();

  private static channelCounter: number = 0;

  constructor(unityCaller: Connector, instanceKey: number) {
    this.unityCaller = unityCaller;
    this.instanceKey = instanceKey;
  }

  makeChannel(address: string): number {
    const channelKey = Instance.channelCounter++;
    const channel = new Channel(channelKey, address, this);
    this.channelMap.set(channelKey, channel);
    return channelKey;
  }

  findChannel(channelKey: number): Channel {
    const channel = this.channelMap.get(channelKey);
    if (!channel)
      throw new Error(`Invalid channelKey: ${channelKey}`);
    return channel;
  }
}

