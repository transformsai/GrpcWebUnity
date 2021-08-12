import Delegator from "./GrpcWebConnector/Delegator";
import TestService from "./service";

const service = new TestService(`http://${window.location.hostname}:${8080}/api`);

service.unary("HELLO");
service.serverStream("HELLO");


(<any>window).GrpcWebUnityDelegator = new Delegator();
