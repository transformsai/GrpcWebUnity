mergeInto(LibraryManager.library, {
  GetObjectName: function () {
    const objName = "GrpcUnityConnector";
    const bufferSize = lengthBytesUTF8(objName) + 1;
    const buffer = _malloc(bufferSize);
    stringToUTF8(objName, buffer, bufferSize);
    return buffer;
  },
  RegisterInstance: function() {
    const unityCaller = {
      Module,
      OnChannelUnaryResponse: function (channelCallMessage) {
        Module.sendMessage("GrpcUnityConnector", "OnChannelUnaryResponse", channelCallMessage);
      },
      OnChannelServerStreamingResponse: function (channelCallMessage) {
        Module.sendMessage("GrpcUnityConnector", "OnChannelServerStreamingResponse", channelCallMessage);
      },
      OnChannelServerStreamingComplete: function (channelCall) {
        Module.sendMessage("GrpcUnityConnector", "OnChannelServerStreamingComplete", channelCall);
      },
      OnChannelCallError: function (channelCallError) {
        Module.sendMessage("GrpcUnityConnector", "OnChannelCallError", channelCallError);
      },
      OnInstanceRegistered: function (instanceKey) {
        Module.sendMessage("GrpcUnityConnector", "OnInstanceRegistered", instanceKey);
      },
      OnInstanceRegistrationFailure: function (errorString) {
        Module.sendMessage("GrpcUnityConnector", "OnInstanceRegistrationFailure", errorString);
      },      
    };
    const register = function() {      
      const instanceKey = window.GrpcWebUnityDelegator.registerUnityInstance(unityCaller);
      unityCaller.OnInstanceRegistered(instanceKey);
      
    }
    if (window.GrpcWebUnityDelegator) {
      register();
      return;
    }
    let scriptTag = document.getElementById("GrpcUnityConnector");
    if (!scriptTag) {
      scriptTag = document.createElement("script");    
      scriptTag.id = "GrpcUnityConnector";
      scriptTag.src = "./bundle.js";
      scriptTag.onload = (event) => register();
      document.body.appendChild(scriptTag);
    } else {
      scriptTag.onload = (event) => register();
    }       
  },  
  RegisterChannel: function(instanceKey, address) {
    return window.GrpcWebUnityDelegator.registerChannel(instanceKey, address);
  },  
  ChannelUnaryRequest: function(instanceKey, channelKey, serviceName, methodName, base64Message) {    
    return window.GrpcWebUnityDelegator.ChannelUnaryRequest(instanceKey, channelKey, serviceName, methodName, base64Message);
  },
  ChannelServerStreamingRequest: function(instanceKey, channelKey, serviceName, methodName, base64Message) {
    return window.GrpcWebUnityDelegator.ChannelServerStreamingRequest(instanceKey, channelKey, serviceName, methodName, base64Message);
  },
  CancelCall: function(instanceKey, channelKey, callKey) {},
});
