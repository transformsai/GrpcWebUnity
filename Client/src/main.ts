import Delegator from "./GrpcWebConnector/Delegator";
// import TestService from "./service";

// const service = new TestService(`http://${window.location.hostname}:${8001}`);

// service.unary("HELLO");
// service.serverStream("HELLO");


(<any>window).GrpcWebUnityDelegator = new Delegator();
