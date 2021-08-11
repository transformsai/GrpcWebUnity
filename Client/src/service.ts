import { GrpcWebFetchTransport } from "@protobuf-ts/grpcweb-transport";
import { TestServiceClient } from "./generated/grpcwebunity/service.client";
import { Request, Response } from "./generated/grpcwebunity/service";

export default class TestService {
  private client: TestServiceClient;

  private transport: GrpcWebFetchTransport;

  constructor(host: string) {
    this.transport = new GrpcWebFetchTransport({ baseUrl: host });
    this.client = new TestServiceClient(this.transport);
  }

  unary(data: string): PromiseLike<Response> {
    const request = Request.create({ data });
    return this.client.unary(request).then(it => it.response);
  }

  serverStream(data: string) {
    const request = Request.create({ data });
    const { responses } = this.client.serverStream(request);
    responses.onMessage(response => {
      console.log(response);
    });
    responses.onError((response) => {
      console.error(response);
    });
  }
}
