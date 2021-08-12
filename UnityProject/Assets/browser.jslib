mergeInto(LibraryManager.library, {
  RegisterInstance: function () {
    const objName = "GrpcWebUnityConnector";

    const register = function () {
      window.GrpcWebUnityDelegator.registerUnityInstance(Module, objName);
    };

    if (window.GrpcWebUnityDelegator) {
      register();
    } else {
      var scriptTag = document.getElementById(objName);
      if (!scriptTag) {
        scriptTag = document.createElement("script");
        scriptTag.id = objName;
        scriptTag.src = "./bundle.js";
        scriptTag.onload = register;
        document.body.appendChild(scriptTag);
      } else {
        scriptTag.onload = register;
      }
    }
    const bufferSize = lengthBytesUTF8(objName) + 1;
    const buffer = _malloc(bufferSize);
    stringToUTF8(objName, buffer, bufferSize);
    return buffer;
  },
  RegisterChannel: function (instanceKey, address) {
    return window.GrpcWebUnityDelegator.registerChannel(instanceKey, address);
  },
  UnaryRequest: function (instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
    return window.GrpcWebUnityDelegator.UnaryRequest(instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
  },
  ServerStreamingRequest: function (instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
    return window.GrpcWebUnityDelegator.ServerStreamingRequest(instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
  },
  CancelCall: function (instanceKey, channelKey, callKey) {
    return window.GrpcWebUnityDelegator.CancelCall(instanceKey, channelKey, callKey);
   },
});
