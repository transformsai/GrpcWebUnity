import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport"

class GrpcDelegator {

    private static counter: number = 0;
    private static channelMap = new Map<number,GrpcDelegatorChannel>();

    registerUnityInstance()

    // makeChannel(target: string): number {
    //     var key = GrpcDelegator.counter++;
    //     GrpcDelegator.channelMap.
    // }
}

class GrpcDelegatorChannel {
    channel: GrpcWebFetchTransport;

    constructor(target: string, GrpcDelegator) {
        this.channel = new GrpcWebFetchTransport({ baseUrl: target });
    }
}

(<any>window).GrpcWebUnityDelegator = new GrpcDelegator();
