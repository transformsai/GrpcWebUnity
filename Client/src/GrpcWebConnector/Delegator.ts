import { Connector } from "./Connector";
import { Instance } from "./Instance";



export default class Delegator {
  private static instanceCounter: number = 0;
  private instanceMap = new Map<number, Instance>();

  findInstance(instanceKey: number): Instance {
    const instance = this.instanceMap.get(instanceKey);
    if (!instance) throw new Error(`Invalid instanceKey: ${instanceKey}`);
    return instance;
  }


  RegisterInstance(unityCaller: UnityPlayer, objectName: string) {
    const handler: ProxyHandler<Connector> = {
      get(target, propName, _receiver) {
        if (propName == "Module") return target.Module;
        return function (param?: string | number) {
          target.Module.SendMessage(objectName, propName.toString(), param);
        }
      }
    }

    const connector = new Proxy(<Connector>{ Module: unityCaller }, handler);

    Delegator.instanceCounter++;
    const instance = new Instance(connector, Delegator.instanceCounter);
    this.instanceMap.set(instance.instanceKey, instance);
    connector.OnInstanceRegistered(instance.instanceKey);
  }

  RegisterChannel(instanceKey: number, address: string): number {
    const instance = this.findInstance(instanceKey);
    return instance.makeChannel(address);
  }

  UnaryRequest(
    instanceKey: number,
    channelKey: number,
    serviceName: string,
    methodName: string,
    headers:string,
    base64Message:string,
    deadlineTimestampSecs:number,
  ): number {
    const instance = this.findInstance(instanceKey);
    const channel = instance.findChannel(channelKey);
    return channel.unaryRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
  }

  ServerStreamingRequest(
    instanceKey: number,
    channelKey: number,
    serviceName: string,
    methodName: string,
    headers:string,
    base64Message:string,
    deadlineTimestampSecs:number
  ): number {
    const instance = this.findInstance(instanceKey);
    const channel = instance.findChannel(channelKey);
    return channel.serverStreamRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
  }
  
  CancelCall(
    instanceKey: number,
    channelKey: number,
    callKey: number
  ){
    const instance = this.findInstance(instanceKey);
    const channel = instance.findChannel(channelKey);
    const call = channel.findCall(callKey);
    call.cancel();
  }

}

