mergeInto(LibraryManager.library, {
  GetObjectName: function () {
    var objName = "GrpcUnityConnector";
    var bufferSize = lengthBytesUTF8(objName) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(objName, buffer, bufferSize);
    return buffer;
  }
})