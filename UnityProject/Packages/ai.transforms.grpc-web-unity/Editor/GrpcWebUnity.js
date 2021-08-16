(() => {
  // node_modules/base64-arraybuffer/dist/base64-arraybuffer.es5.js
  var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
  var lookup = typeof Uint8Array === "undefined" ? [] : new Uint8Array(256);
  for (i = 0; i < chars.length; i++) {
    lookup[chars.charCodeAt(i)] = i;
  }
  var i;
  var encode = function(arraybuffer) {
    var bytes = new Uint8Array(arraybuffer), i, len = bytes.length, base64 = "";
    for (i = 0; i < len; i += 3) {
      base64 += chars[bytes[i] >> 2];
      base64 += chars[(bytes[i] & 3) << 4 | bytes[i + 1] >> 4];
      base64 += chars[(bytes[i + 1] & 15) << 2 | bytes[i + 2] >> 6];
      base64 += chars[bytes[i + 2] & 63];
    }
    if (len % 3 === 2) {
      base64 = base64.substring(0, base64.length - 1) + "=";
    } else if (len % 3 === 1) {
      base64 = base64.substring(0, base64.length - 2) + "==";
    }
    return base64;
  };
  var decode = function(base64) {
    var bufferLength = base64.length * 0.75, len = base64.length, i, p = 0, encoded1, encoded2, encoded3, encoded4;
    if (base64[base64.length - 1] === "=") {
      bufferLength--;
      if (base64[base64.length - 2] === "=") {
        bufferLength--;
      }
    }
    var arraybuffer = new ArrayBuffer(bufferLength), bytes = new Uint8Array(arraybuffer);
    for (i = 0; i < len; i += 4) {
      encoded1 = lookup[base64.charCodeAt(i)];
      encoded2 = lookup[base64.charCodeAt(i + 1)];
      encoded3 = lookup[base64.charCodeAt(i + 2)];
      encoded4 = lookup[base64.charCodeAt(i + 3)];
      bytes[p++] = encoded1 << 2 | encoded2 >> 4;
      bytes[p++] = (encoded2 & 15) << 4 | encoded3 >> 2;
      bytes[p++] = (encoded3 & 3) << 6 | encoded4 & 63;
    }
    return arraybuffer;
  };

  // node_modules/@protobuf-ts/runtime/build/es2015/base64.js
  var encTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".split("");
  var decTable = [];
  for (let i = 0; i < encTable.length; i++)
    decTable[encTable[i].charCodeAt(0)] = i;
  decTable["-".charCodeAt(0)] = encTable.indexOf("+");
  decTable["_".charCodeAt(0)] = encTable.indexOf("/");
  function base64decode(base64Str) {
    let es = base64Str.length * 3 / 4;
    if (base64Str[base64Str.length - 2] == "=")
      es -= 2;
    else if (base64Str[base64Str.length - 1] == "=")
      es -= 1;
    let bytes = new Uint8Array(es), bytePos = 0, groupPos = 0, b, p = 0;
    for (let i = 0; i < base64Str.length; i++) {
      b = decTable[base64Str.charCodeAt(i)];
      if (b === void 0) {
        switch (base64Str[i]) {
          case "=":
            groupPos = 0;
          case "\n":
          case "\r":
          case "	":
          case " ":
            continue;
          default:
            throw Error(`invalid base64 string.`);
        }
      }
      switch (groupPos) {
        case 0:
          p = b;
          groupPos = 1;
          break;
        case 1:
          bytes[bytePos++] = p << 2 | (b & 48) >> 4;
          p = b;
          groupPos = 2;
          break;
        case 2:
          bytes[bytePos++] = (p & 15) << 4 | (b & 60) >> 2;
          p = b;
          groupPos = 3;
          break;
        case 3:
          bytes[bytePos++] = (p & 3) << 6 | b;
          groupPos = 0;
          break;
      }
    }
    if (groupPos == 1)
      throw Error(`invalid base64 string.`);
    return bytes.subarray(0, bytePos);
  }
  function base64encode(bytes) {
    let base64 = "", groupPos = 0, b, p = 0;
    for (let i = 0; i < bytes.length; i++) {
      b = bytes[i];
      switch (groupPos) {
        case 0:
          base64 += encTable[b >> 2];
          p = (b & 3) << 4;
          groupPos = 1;
          break;
        case 1:
          base64 += encTable[p | b >> 4];
          p = (b & 15) << 2;
          groupPos = 2;
          break;
        case 2:
          base64 += encTable[p | b >> 6];
          base64 += encTable[b & 63];
          groupPos = 0;
          break;
      }
    }
    if (groupPos) {
      base64 += encTable[p];
      base64 += "=";
      if (groupPos == 1)
        base64 += "=";
    }
    return base64;
  }

  // node_modules/@protobuf-ts/runtime/build/es2015/assert.js
  function assert(condition, msg) {
    if (!condition) {
      throw new Error(msg);
    }
  }

  // node_modules/@protobuf-ts/runtime/build/es2015/binary-format-contract.js
  var UnknownFieldHandler;
  (function(UnknownFieldHandler2) {
    UnknownFieldHandler2.symbol = Symbol("protobuf-ts/unknown");
    UnknownFieldHandler2.onRead = (typeName, message, fieldNo, wireType, data) => {
      let container = is(message) ? message[UnknownFieldHandler2.symbol] : message[UnknownFieldHandler2.symbol] = [];
      container.push({ no: fieldNo, wireType, data });
    };
    UnknownFieldHandler2.onWrite = (typeName, message, writer) => {
      for (let { no, wireType, data } of UnknownFieldHandler2.list(message))
        writer.tag(no, wireType).raw(data);
    };
    UnknownFieldHandler2.list = (message, fieldNo) => {
      if (is(message)) {
        let all = message[UnknownFieldHandler2.symbol];
        return fieldNo ? all.filter((uf) => uf.no == fieldNo) : all;
      }
      return [];
    };
    UnknownFieldHandler2.last = (message, fieldNo) => UnknownFieldHandler2.list(message, fieldNo).slice(-1)[0];
    const is = (message) => message && Array.isArray(message[UnknownFieldHandler2.symbol]);
  })(UnknownFieldHandler || (UnknownFieldHandler = {}));
  function mergeBinaryOptions(a, b) {
    return Object.assign(Object.assign({}, a), b);
  }
  var WireType;
  (function(WireType2) {
    WireType2[WireType2["Varint"] = 0] = "Varint";
    WireType2[WireType2["Bit64"] = 1] = "Bit64";
    WireType2[WireType2["LengthDelimited"] = 2] = "LengthDelimited";
    WireType2[WireType2["StartGroup"] = 3] = "StartGroup";
    WireType2[WireType2["EndGroup"] = 4] = "EndGroup";
    WireType2[WireType2["Bit32"] = 5] = "Bit32";
  })(WireType || (WireType = {}));

  // node_modules/@protobuf-ts/runtime/build/es2015/json-format-contract.js
  function mergeJsonOptions(a, b) {
    var _a, _b;
    let c = Object.assign(Object.assign({}, a), b);
    c.typeRegistry = [...(_a = a === null || a === void 0 ? void 0 : a.typeRegistry) !== null && _a !== void 0 ? _a : [], ...(_b = b === null || b === void 0 ? void 0 : b.typeRegistry) !== null && _b !== void 0 ? _b : []];
    return c;
  }

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/rpc-error.js
  var RpcError = class extends Error {
    constructor(message, code = "UNKNOWN", meta) {
      super(message);
      this.name = "RpcError";
      Object.setPrototypeOf(this, new.target.prototype);
      this.code = code;
      this.meta = meta !== null && meta !== void 0 ? meta : {};
    }
    toString() {
      const l = [this.name + ": " + this.message];
      if (this.code) {
        l.push("");
        l.push("Code: " + this.code);
      }
      let m = Object.entries(this.meta);
      if (m.length) {
        l.push("");
        l.push("Meta:");
        for (let [k, v] of m) {
          l.push(`  ${k}: ${v}`);
        }
      }
      return l.join("\n");
    }
  };

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/rpc-options.js
  function mergeRpcOptions(defaults, options) {
    if (!options)
      return defaults;
    let o = {};
    copy(defaults, o);
    copy(options, o);
    for (let key of Object.keys(options)) {
      let val = options[key];
      switch (key) {
        case "jsonOptions":
          o.jsonOptions = mergeJsonOptions(defaults.jsonOptions, o.jsonOptions);
          break;
        case "binaryOptions":
          o.binaryOptions = mergeBinaryOptions(defaults.binaryOptions, o.binaryOptions);
          break;
        case "meta":
          o.meta = {};
          copy(defaults.meta, o.meta);
          copy(options.meta, o.meta);
          break;
        case "interceptors":
          o.interceptors = defaults.interceptors ? defaults.interceptors.concat(val) : val.concat();
          break;
      }
    }
    return o;
  }
  function copy(a, into) {
    if (!a)
      return;
    let c = into;
    for (let [k, v] of Object.entries(a)) {
      if (v instanceof Date)
        c[k] = new Date(v.getTime());
      else if (Array.isArray(v))
        c[k] = v.concat();
      else
        c[k] = v;
    }
  }

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/deferred.js
  var DeferredState;
  (function(DeferredState2) {
    DeferredState2[DeferredState2["PENDING"] = 0] = "PENDING";
    DeferredState2[DeferredState2["REJECTED"] = 1] = "REJECTED";
    DeferredState2[DeferredState2["RESOLVED"] = 2] = "RESOLVED";
  })(DeferredState || (DeferredState = {}));
  var Deferred = class {
    constructor(preventUnhandledRejectionWarning = true) {
      this._state = DeferredState.PENDING;
      this._promise = new Promise((resolve, reject) => {
        this._resolve = resolve;
        this._reject = reject;
      });
      if (preventUnhandledRejectionWarning) {
        this._promise.catch((_) => {
        });
      }
    }
    get state() {
      return this._state;
    }
    get promise() {
      return this._promise;
    }
    resolve(value) {
      if (this.state !== DeferredState.PENDING)
        throw new Error(`cannot resolve ${DeferredState[this.state].toLowerCase()}`);
      this._resolve(value);
      this._state = DeferredState.RESOLVED;
    }
    reject(reason) {
      if (this.state !== DeferredState.PENDING)
        throw new Error(`cannot reject ${DeferredState[this.state].toLowerCase()}`);
      this._reject(reason);
      this._state = DeferredState.REJECTED;
    }
    resolvePending(val) {
      if (this._state === DeferredState.PENDING)
        this.resolve(val);
    }
    rejectPending(reason) {
      if (this._state === DeferredState.PENDING)
        this.reject(reason);
    }
  };

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/rpc-output-stream.js
  var RpcOutputStreamController = class {
    constructor() {
      this._lis = {
        nxt: [],
        msg: [],
        err: [],
        cmp: []
      };
      this._closed = false;
    }
    onNext(callback) {
      return this.addLis(callback, this._lis.nxt);
    }
    onMessage(callback) {
      return this.addLis(callback, this._lis.msg);
    }
    onError(callback) {
      return this.addLis(callback, this._lis.err);
    }
    onComplete(callback) {
      return this.addLis(callback, this._lis.cmp);
    }
    addLis(callback, list) {
      list.push(callback);
      return () => {
        let i = list.indexOf(callback);
        if (i >= 0)
          list.splice(i, 1);
      };
    }
    clearLis() {
      for (let l of Object.values(this._lis))
        l.splice(0, l.length);
    }
    get closed() {
      return this._closed !== false;
    }
    notifyNext(message, error, complete) {
      assert((message ? 1 : 0) + (error ? 1 : 0) + (complete ? 1 : 0) <= 1, "only one emission at a time");
      if (message)
        this.notifyMessage(message);
      if (error)
        this.notifyError(error);
      if (complete)
        this.notifyComplete();
    }
    notifyMessage(message) {
      assert(!this.closed, "stream is closed");
      this.pushIt({ value: message, done: false });
      this._lis.msg.forEach((l) => l(message));
      this._lis.nxt.forEach((l) => l(message, void 0, false));
    }
    notifyError(error) {
      assert(!this.closed, "stream is closed");
      this._closed = error;
      this.pushIt(error);
      this._lis.err.forEach((l) => l(error));
      this._lis.nxt.forEach((l) => l(void 0, error, false));
      this.clearLis();
    }
    notifyComplete() {
      assert(!this.closed, "stream is closed");
      this._closed = true;
      this.pushIt({ value: null, done: true });
      this._lis.cmp.forEach((l) => l());
      this._lis.nxt.forEach((l) => l(void 0, void 0, true));
      this.clearLis();
    }
    [Symbol.asyncIterator]() {
      if (!this._itState) {
        this._itState = { q: [] };
      }
      if (this._closed === true)
        this.pushIt({ value: null, done: true });
      else if (this._closed !== false)
        this.pushIt(this._closed);
      return {
        next: () => {
          let state = this._itState;
          assert(state, "bad state");
          assert(!state.p, "iterator contract broken");
          let first = state.q.shift();
          if (first)
            return "value" in first ? Promise.resolve(first) : Promise.reject(first);
          state.p = new Deferred();
          return state.p.promise;
        }
      };
    }
    pushIt(result) {
      let state = this._itState;
      if (!state)
        return;
      if (state.p) {
        const p = state.p;
        assert(p.state == DeferredState.PENDING, "iterator contract broken");
        "value" in result ? p.resolve(result) : p.reject(result);
        delete state.p;
      } else {
        state.q.push(result);
      }
    }
  };

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/unary-call.js
  var __awaiter = function(thisArg, _arguments, P, generator) {
    function adopt(value) {
      return value instanceof P ? value : new P(function(resolve) {
        resolve(value);
      });
    }
    return new (P || (P = Promise))(function(resolve, reject) {
      function fulfilled(value) {
        try {
          step(generator.next(value));
        } catch (e) {
          reject(e);
        }
      }
      function rejected(value) {
        try {
          step(generator["throw"](value));
        } catch (e) {
          reject(e);
        }
      }
      function step(result) {
        result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected);
      }
      step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
  };
  var UnaryCall = class {
    constructor(method, requestHeaders, request, headers, response, status, trailers) {
      this.method = method;
      this.requestHeaders = requestHeaders;
      this.request = request;
      this.headers = headers;
      this.response = response;
      this.status = status;
      this.trailers = trailers;
    }
    then(onfulfilled, onrejected) {
      return this.promiseFinished().then((value) => onfulfilled ? Promise.resolve(onfulfilled(value)) : value, (reason) => onrejected ? Promise.resolve(onrejected(reason)) : Promise.reject(reason));
    }
    promiseFinished() {
      return __awaiter(this, void 0, void 0, function* () {
        let [headers, response, status, trailers] = yield Promise.all([this.headers, this.response, this.status, this.trailers]);
        return {
          method: this.method,
          requestHeaders: this.requestHeaders,
          request: this.request,
          headers,
          response,
          status,
          trailers
        };
      });
    }
  };

  // node_modules/@protobuf-ts/runtime-rpc/build/es2015/server-streaming-call.js
  var __awaiter2 = function(thisArg, _arguments, P, generator) {
    function adopt(value) {
      return value instanceof P ? value : new P(function(resolve) {
        resolve(value);
      });
    }
    return new (P || (P = Promise))(function(resolve, reject) {
      function fulfilled(value) {
        try {
          step(generator.next(value));
        } catch (e) {
          reject(e);
        }
      }
      function rejected(value) {
        try {
          step(generator["throw"](value));
        } catch (e) {
          reject(e);
        }
      }
      function step(result) {
        result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected);
      }
      step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
  };
  var ServerStreamingCall = class {
    constructor(method, requestHeaders, request, headers, response, status, trailers) {
      this.method = method;
      this.requestHeaders = requestHeaders;
      this.request = request;
      this.headers = headers;
      this.responses = response;
      this.status = status;
      this.trailers = trailers;
    }
    then(onfulfilled, onrejected) {
      return this.promiseFinished().then((value) => onfulfilled ? Promise.resolve(onfulfilled(value)) : value, (reason) => onrejected ? Promise.resolve(onrejected(reason)) : Promise.reject(reason));
    }
    promiseFinished() {
      return __awaiter2(this, void 0, void 0, function* () {
        let [headers, status, trailers] = yield Promise.all([this.headers, this.status, this.trailers]);
        return {
          method: this.method,
          requestHeaders: this.requestHeaders,
          request: this.request,
          headers,
          status,
          trailers
        };
      });
    }
  };

  // node_modules/@protobuf-ts/grpcweb-transport/build/es2015/goog-grpc-status-code.js
  var GrpcStatusCode;
  (function(GrpcStatusCode2) {
    GrpcStatusCode2[GrpcStatusCode2["OK"] = 0] = "OK";
    GrpcStatusCode2[GrpcStatusCode2["CANCELLED"] = 1] = "CANCELLED";
    GrpcStatusCode2[GrpcStatusCode2["UNKNOWN"] = 2] = "UNKNOWN";
    GrpcStatusCode2[GrpcStatusCode2["INVALID_ARGUMENT"] = 3] = "INVALID_ARGUMENT";
    GrpcStatusCode2[GrpcStatusCode2["DEADLINE_EXCEEDED"] = 4] = "DEADLINE_EXCEEDED";
    GrpcStatusCode2[GrpcStatusCode2["NOT_FOUND"] = 5] = "NOT_FOUND";
    GrpcStatusCode2[GrpcStatusCode2["ALREADY_EXISTS"] = 6] = "ALREADY_EXISTS";
    GrpcStatusCode2[GrpcStatusCode2["PERMISSION_DENIED"] = 7] = "PERMISSION_DENIED";
    GrpcStatusCode2[GrpcStatusCode2["UNAUTHENTICATED"] = 16] = "UNAUTHENTICATED";
    GrpcStatusCode2[GrpcStatusCode2["RESOURCE_EXHAUSTED"] = 8] = "RESOURCE_EXHAUSTED";
    GrpcStatusCode2[GrpcStatusCode2["FAILED_PRECONDITION"] = 9] = "FAILED_PRECONDITION";
    GrpcStatusCode2[GrpcStatusCode2["ABORTED"] = 10] = "ABORTED";
    GrpcStatusCode2[GrpcStatusCode2["OUT_OF_RANGE"] = 11] = "OUT_OF_RANGE";
    GrpcStatusCode2[GrpcStatusCode2["UNIMPLEMENTED"] = 12] = "UNIMPLEMENTED";
    GrpcStatusCode2[GrpcStatusCode2["INTERNAL"] = 13] = "INTERNAL";
    GrpcStatusCode2[GrpcStatusCode2["UNAVAILABLE"] = 14] = "UNAVAILABLE";
    GrpcStatusCode2[GrpcStatusCode2["DATA_LOSS"] = 15] = "DATA_LOSS";
  })(GrpcStatusCode || (GrpcStatusCode = {}));

  // node_modules/@protobuf-ts/grpcweb-transport/build/es2015/grpc-web-format.js
  var __awaiter3 = function(thisArg, _arguments, P, generator) {
    function adopt(value) {
      return value instanceof P ? value : new P(function(resolve) {
        resolve(value);
      });
    }
    return new (P || (P = Promise))(function(resolve, reject) {
      function fulfilled(value) {
        try {
          step(generator.next(value));
        } catch (e) {
          reject(e);
        }
      }
      function rejected(value) {
        try {
          step(generator["throw"](value));
        } catch (e) {
          reject(e);
        }
      }
      function step(result) {
        result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected);
      }
      step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
  };
  function createGrpcWebRequestHeader(headers, format, timeout, meta, userAgent) {
    if (meta) {
      for (let [k, v] of Object.entries(meta)) {
        if (typeof v == "string")
          headers.append(k, v);
        else
          for (let i of v)
            headers.append(k, i);
      }
    }
    headers.set("Content-Type", format === "text" ? "application/grpc-web-text" : "application/grpc-web+proto");
    if (format == "text") {
      headers.set("Accept", "application/grpc-web-text");
    }
    headers.set("X-Grpc-Web", "1");
    if (userAgent)
      headers.set("X-User-Agent", userAgent);
    if (typeof timeout === "number") {
      if (timeout <= 0) {
        throw new RpcError(`timeout ${timeout} ms exceeded`, GrpcStatusCode[GrpcStatusCode.DEADLINE_EXCEEDED]);
      }
      headers.set("grpc-timeout", `${timeout}m`);
    } else if (timeout) {
      const deadline = timeout.getTime();
      const now = Date.now();
      if (deadline <= now) {
        throw new RpcError(`deadline ${timeout} exceeded`, GrpcStatusCode[GrpcStatusCode.DEADLINE_EXCEEDED]);
      }
      headers.set("grpc-timeout", `${deadline - now}m`);
    }
    return headers;
  }
  function createGrpcWebRequestBody(message, format) {
    let body = new Uint8Array(5 + message.length);
    body[0] = GrpcWebFrame.DATA;
    for (let msgLen = message.length, i = 4; i > 0; i--) {
      body[i] = msgLen % 256;
      msgLen >>>= 8;
    }
    body.set(message, 5);
    return format === "binary" ? body : base64encode(body);
  }
  function readGrpcWebResponseHeader(headersOrFetchResponse, httpStatus, httpStatusText) {
    if (arguments.length === 1) {
      let fetchResponse = headersOrFetchResponse;
      switch (fetchResponse.type) {
        case "error":
        case "opaque":
        case "opaqueredirect":
          throw new RpcError(`fetch response type ${fetchResponse.type}`, GrpcStatusCode[GrpcStatusCode.UNKNOWN]);
      }
      return readGrpcWebResponseHeader(fetchHeadersToHttp(fetchResponse.headers), fetchResponse.status, fetchResponse.statusText);
    }
    let headers = headersOrFetchResponse, httpOk = httpStatus >= 200 && httpStatus < 300, responseMeta = parseMetadata(headers), [statusCode, statusDetail] = parseStatus(headers);
    if (statusCode === GrpcStatusCode.OK && !httpOk) {
      statusCode = httpStatusToGrpc(httpStatus);
      statusDetail = httpStatusText;
    }
    return [statusCode, statusDetail, responseMeta];
  }
  function readGrpcWebResponseTrailer(data) {
    let headers = parseTrailer(data), [code, detail] = parseStatus(headers), meta = parseMetadata(headers);
    return [code, detail, meta];
  }
  var GrpcWebFrame;
  (function(GrpcWebFrame2) {
    GrpcWebFrame2[GrpcWebFrame2["DATA"] = 0] = "DATA";
    GrpcWebFrame2[GrpcWebFrame2["TRAILER"] = 128] = "TRAILER";
  })(GrpcWebFrame || (GrpcWebFrame = {}));
  function readGrpcWebResponseBody(stream, contentType, onFrame) {
    return __awaiter3(this, void 0, void 0, function* () {
      let streamReader, base64queue = "", byteQueue = new Uint8Array(0), format = parseFormat(contentType);
      if (typeof stream.getReader == "function") {
        let whatWgReadableStream = stream.getReader();
        streamReader = {
          next: () => whatWgReadableStream.read()
        };
      } else {
        streamReader = stream[Symbol.asyncIterator]();
      }
      while (true) {
        let result = yield streamReader.next();
        if (result.value !== void 0) {
          if (format === "text") {
            for (let i = 0; i < result.value.length; i++)
              base64queue += String.fromCharCode(result.value[i]);
            let safeLen = base64queue.length - base64queue.length % 4;
            if (safeLen === 0)
              continue;
            byteQueue = concatBytes(byteQueue, base64decode(base64queue.substring(0, safeLen)));
            base64queue = base64queue.substring(safeLen);
          } else {
            byteQueue = concatBytes(byteQueue, result.value);
          }
          while (byteQueue.length >= 5 && byteQueue[0] === GrpcWebFrame.DATA) {
            let msgLen = 0;
            for (let i = 1; i < 5; i++)
              msgLen = (msgLen << 8) + byteQueue[i];
            if (byteQueue.length - 5 >= msgLen) {
              onFrame(GrpcWebFrame.DATA, byteQueue.subarray(5, 5 + msgLen));
              byteQueue = byteQueue.subarray(5 + msgLen);
            } else
              break;
          }
        }
        if (result.done) {
          if (byteQueue.length === 0)
            break;
          if (byteQueue[0] !== GrpcWebFrame.TRAILER || byteQueue.length < 5)
            throw new RpcError("premature EOF", GrpcStatusCode[GrpcStatusCode.DATA_LOSS]);
          onFrame(GrpcWebFrame.TRAILER, byteQueue.subarray(5));
          break;
        }
      }
    });
  }
  function concatBytes(a, b) {
    let n = new Uint8Array(a.length + b.length);
    n.set(a);
    n.set(b, a.length);
    return n;
  }
  function parseFormat(contentType) {
    switch (contentType) {
      case "application/grpc-web-text":
      case "application/grpc-web-text+proto":
        return "text";
      case "application/grpc-web":
      case "application/grpc-web+proto":
        return "binary";
      case void 0:
      case null:
        throw new RpcError("missing response content type", GrpcStatusCode[GrpcStatusCode.INTERNAL]);
      default:
        throw new RpcError("unexpected response content type: " + contentType, GrpcStatusCode[GrpcStatusCode.INTERNAL]);
    }
  }
  function parseStatus(headers) {
    let code = GrpcStatusCode.OK, message;
    let m = headers["grpc-message"];
    if (m !== void 0) {
      if (Array.isArray(m))
        return [GrpcStatusCode.INTERNAL, "invalid grpc-web message"];
      message = m;
    }
    let s = headers["grpc-status"];
    if (s !== void 0) {
      if (Array.isArray(m) || GrpcStatusCode[code] === void 0)
        return [GrpcStatusCode.INTERNAL, "invalid grpc-web status"];
      code = parseInt(s);
    }
    return [code, message];
  }
  function parseMetadata(headers) {
    let meta = {};
    for (let [k, v] of Object.entries(headers))
      switch (k) {
        case "grpc-message":
        case "grpc-status":
        case "content-type":
          break;
        default:
          meta[k] = v;
      }
    return meta;
  }
  function parseTrailer(trailerData) {
    let headers = {};
    for (let chunk of String.fromCharCode.apply(String, trailerData).trim().split("\r\n")) {
      let [key, value] = chunk.split(":", 2);
      key = key.trim();
      value = value.trim();
      let e = headers[key];
      if (typeof e == "string")
        headers[key] = [e, value];
      else if (Array.isArray(e))
        e.push(value);
      else
        headers[key] = value;
    }
    return headers;
  }
  function fetchHeadersToHttp(fetchHeaders) {
    let headers = {};
    fetchHeaders.forEach((value, key) => {
      let e = headers[key];
      if (typeof e == "string")
        headers[key] = [e, value];
      else if (Array.isArray(e))
        e.push(value);
      else
        headers[key] = value;
    });
    return headers;
  }
  function httpStatusToGrpc(httpStatus) {
    switch (httpStatus) {
      case 200:
        return GrpcStatusCode.OK;
      case 400:
        return GrpcStatusCode.INVALID_ARGUMENT;
      case 401:
        return GrpcStatusCode.UNAUTHENTICATED;
      case 403:
        return GrpcStatusCode.PERMISSION_DENIED;
      case 404:
        return GrpcStatusCode.NOT_FOUND;
      case 409:
        return GrpcStatusCode.ABORTED;
      case 412:
        return GrpcStatusCode.FAILED_PRECONDITION;
      case 429:
        return GrpcStatusCode.RESOURCE_EXHAUSTED;
      case 499:
        return GrpcStatusCode.CANCELLED;
      case 500:
        return GrpcStatusCode.UNKNOWN;
      case 501:
        return GrpcStatusCode.UNIMPLEMENTED;
      case 503:
        return GrpcStatusCode.UNAVAILABLE;
      case 504:
        return GrpcStatusCode.DEADLINE_EXCEEDED;
      default:
        return GrpcStatusCode.UNKNOWN;
    }
  }

  // node_modules/@protobuf-ts/grpcweb-transport/build/es2015/grpc-web-transport.js
  var GrpcWebFetchTransport = class {
    constructor(defaultOptions) {
      this.defaultOptions = defaultOptions;
    }
    mergeOptions(options) {
      return mergeRpcOptions(this.defaultOptions, options);
    }
    makeUrl(method, options) {
      let base = options.baseUrl;
      if (base.endsWith("/"))
        base = base.substring(0, base.length - 1);
      return `${base}/${method.service.typeName}/${method.name}`;
    }
    clientStreaming() {
      throw new RpcError("Client streaming is not supported by grpc-web", GrpcStatusCode[GrpcStatusCode.UNIMPLEMENTED]);
    }
    duplex() {
      throw new RpcError("Duplex streaming is not supported by grpc-web", GrpcStatusCode[GrpcStatusCode.UNIMPLEMENTED]);
    }
    serverStreaming(method, input, options) {
      var _a, _b, _c, _d;
      let opt = options, format = (_a = opt.format) !== null && _a !== void 0 ? _a : "text", fetchInit = (_b = opt.fetchInit) !== null && _b !== void 0 ? _b : {}, url = this.makeUrl(method, opt), inputBytes = method.I.toBinary(input, opt.binaryOptions), defHeader = new Deferred(), responseStream = new RpcOutputStreamController(), maybeStatus, defStatus = new Deferred(), maybeTrailer, defTrailer = new Deferred();
      globalThis.fetch(url, Object.assign(Object.assign({}, fetchInit), {
        method: "POST",
        headers: createGrpcWebRequestHeader(new globalThis.Headers(), format, opt.timeout, opt.meta),
        body: createGrpcWebRequestBody(inputBytes, format),
        signal: (_c = options.abort) !== null && _c !== void 0 ? _c : null
      })).then((fetchResponse) => {
        let [code, detail, meta] = readGrpcWebResponseHeader(fetchResponse);
        defHeader.resolve(meta);
        if (code !== GrpcStatusCode.OK)
          throw new RpcError(detail !== null && detail !== void 0 ? detail : GrpcStatusCode[code], GrpcStatusCode[code], meta);
        return fetchResponse;
      }).then((fetchResponse) => {
        if (!fetchResponse.body)
          throw new RpcError("missing response body", GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        return readGrpcWebResponseBody(fetchResponse.body, fetchResponse.headers.get("content-type"), (type, data) => {
          switch (type) {
            case GrpcWebFrame.DATA:
              responseStream.notifyMessage(method.O.fromBinary(data, opt.binaryOptions));
              break;
            case GrpcWebFrame.TRAILER:
              let code, detail;
              [code, detail, maybeTrailer] = readGrpcWebResponseTrailer(data);
              maybeStatus = {
                code: GrpcStatusCode[code],
                detail: detail !== null && detail !== void 0 ? detail : GrpcStatusCode[code]
              };
              break;
          }
        });
      }).then(() => {
        if (!maybeTrailer)
          throw new RpcError(`missing trailers`, GrpcStatusCode[GrpcStatusCode.DATA_LOSS]);
        if (!maybeStatus)
          throw new RpcError(`missing status`, GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        if (maybeStatus.code !== "OK")
          throw new RpcError(maybeStatus.detail, maybeStatus.code, maybeTrailer);
        responseStream.notifyComplete();
        defStatus.resolve(maybeStatus);
        defTrailer.resolve(maybeTrailer);
      }).catch((reason) => {
        let error;
        if (reason instanceof RpcError)
          error = reason;
        else if (reason instanceof Error && reason.name === "AbortError")
          error = new RpcError(reason.message, GrpcStatusCode[GrpcStatusCode.CANCELLED]);
        else
          error = new RpcError(reason instanceof Error ? reason.message : "" + reason, GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        defHeader.rejectPending(error);
        responseStream.notifyError(error);
        defStatus.rejectPending(error);
        defTrailer.rejectPending(error);
      });
      return new ServerStreamingCall(method, (_d = opt.meta) !== null && _d !== void 0 ? _d : {}, input, defHeader.promise, responseStream, defStatus.promise, defTrailer.promise);
    }
    unary(method, input, options) {
      var _a, _b, _c, _d;
      let opt = options, format = (_a = opt.format) !== null && _a !== void 0 ? _a : "text", fetchInit = (_b = opt.fetchInit) !== null && _b !== void 0 ? _b : {}, url = this.makeUrl(method, opt), inputBytes = method.I.toBinary(input, opt.binaryOptions), defHeader = new Deferred(), maybeMessage, defMessage = new Deferred(), maybeStatus, defStatus = new Deferred(), maybeTrailer, defTrailer = new Deferred();
      globalThis.fetch(url, Object.assign(Object.assign({}, fetchInit), {
        method: "POST",
        headers: createGrpcWebRequestHeader(new globalThis.Headers(), format, opt.timeout, opt.meta),
        body: createGrpcWebRequestBody(inputBytes, format),
        signal: (_c = options.abort) !== null && _c !== void 0 ? _c : null
      })).then((fetchResponse) => {
        let [code, detail, meta] = readGrpcWebResponseHeader(fetchResponse);
        defHeader.resolve(meta);
        if (code !== GrpcStatusCode.OK)
          throw new RpcError(detail !== null && detail !== void 0 ? detail : GrpcStatusCode[code], GrpcStatusCode[code], meta);
        return fetchResponse;
      }).then((fetchResponse) => {
        if (!fetchResponse.body)
          throw new RpcError("missing response body", GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        return readGrpcWebResponseBody(fetchResponse.body, fetchResponse.headers.get("content-type"), (type, data) => {
          switch (type) {
            case GrpcWebFrame.DATA:
              if (defMessage.state === DeferredState.RESOLVED)
                throw new RpcError(`unary call received 2nd message`, GrpcStatusCode[GrpcStatusCode.DATA_LOSS]);
              maybeMessage = method.O.fromBinary(data, opt.binaryOptions);
              break;
            case GrpcWebFrame.TRAILER:
              let code, detail;
              [code, detail, maybeTrailer] = readGrpcWebResponseTrailer(data);
              maybeStatus = {
                code: GrpcStatusCode[code],
                detail: detail !== null && detail !== void 0 ? detail : GrpcStatusCode[code]
              };
              break;
          }
        });
      }).then(() => {
        if (!maybeTrailer)
          throw new RpcError(`missing trailers`, GrpcStatusCode[GrpcStatusCode.DATA_LOSS]);
        if (!maybeStatus)
          throw new RpcError(`missing status`, GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        if (!maybeMessage && maybeStatus.code === "OK")
          throw new RpcError("expected error status", GrpcStatusCode[GrpcStatusCode.DATA_LOSS]);
        if (!maybeMessage)
          throw new RpcError(maybeStatus.detail, maybeStatus.code, maybeTrailer);
        defMessage.resolve(maybeMessage);
        if (maybeStatus.code !== "OK")
          throw new RpcError(maybeStatus.detail, maybeStatus.code, maybeTrailer);
        defStatus.resolve(maybeStatus);
        defTrailer.resolve(maybeTrailer);
      }).catch((reason) => {
        let error;
        if (reason instanceof RpcError)
          error = reason;
        else if (reason instanceof Error && reason.name === "AbortError")
          error = new RpcError(reason.message, GrpcStatusCode[GrpcStatusCode.CANCELLED]);
        else
          error = new RpcError(reason instanceof Error ? reason.message : "" + reason, GrpcStatusCode[GrpcStatusCode.INTERNAL]);
        defHeader.rejectPending(error);
        defMessage.rejectPending(error);
        defStatus.rejectPending(error);
        defTrailer.rejectPending(error);
      });
      return new UnaryCall(method, (_d = opt.meta) !== null && _d !== void 0 ? _d : {}, input, defHeader.promise, defMessage.promise, defStatus.promise, defTrailer.promise);
    }
  };

  // src/GrpcWebConnector/UnityProtoMessage.ts
  var UnityProtoMessageType = class {
    constructor() {
      this.typeName = "UnityMessage";
      this.fields = [];
      this.options = {};
    }
    create(value) {
      if (!value)
        return { bytes: new Uint8Array(0) };
      throw new Error("Method not implemented.");
    }
    fromBinary(data, _options) {
      return { bytes: new Uint8Array(data) };
    }
    toBinary(message, _options) {
      return new Uint8Array(message.bytes);
    }
    fromJson(_json, _options) {
      throw new Error("Method not implemented.");
    }
    fromJsonString(_json, _options) {
      throw new Error("Method not implemented.");
    }
    toJson(_message, _options) {
      throw new Error("Method not implemented.");
    }
    toJsonString(_message, _options) {
      throw new Error("Method not implemented.");
    }
    clone(message) {
      return { bytes: new Uint8Array(message.bytes) };
    }
    mergePartial(_target, _source) {
      throw new Error("Method not implemented.");
    }
    equals(a, b) {
      return Boolean((a == null ? void 0 : a.bytes.length) === (b == null ? void 0 : b.bytes.length) && (a == null ? void 0 : a.bytes.every((v, i) => v === (b == null ? void 0 : b.bytes[i]))));
    }
    is(_arg, _depth) {
      throw new Error("Method not implemented.");
    }
    isAssignable(_arg, _depth) {
      throw new Error("Method not implemented.");
    }
    internalJsonRead(_json, _options, _target) {
      throw new Error("Method not implemented.");
    }
    internalJsonWrite(_message, _options) {
      throw new Error("Method not implemented.");
    }
    internalBinaryWrite(_message, _writer, _options) {
      throw new Error("Method not implemented.");
    }
    internalBinaryRead(_reader, _length, _options, _target) {
      throw new Error("Method not implemented.");
    }
  };
  var UnityProtoMessage = new UnityProtoMessageType();

  // src/GrpcWebConnector/Utils.ts
  var utf8Encoder = new TextEncoder();
  var utf8Decoder = new TextDecoder();
  function toBase64(str) {
    var bytes = utf8Encoder.encode(str);
    return encode(bytes.buffer);
  }
  function fromBase64(str) {
    var bytes = decode(str);
    return utf8Decoder.decode(bytes);
  }
  var BinarySuffix = "-bin";
  function EncodeMetadata(metadata) {
    if (!metadata)
      return "";
    var entries = Object.entries(metadata);
    if (entries.length == 0)
      return "";
    var builder = "";
    for (var entry of entries) {
      var key = entry[0];
      var values;
      if (Array.isArray(entry[1]))
        values = entry[1];
      else
        values = [entry[1]];
      for (var value of values) {
        builder += toBase64(key);
        builder += "|";
        if (key.endsWith(BinarySuffix))
          builder += value;
        else
          builder += toBase64(value);
      }
    }
    return builder;
  }
  function DecodeMetadata(str) {
    if (!str)
      return void 0;
    var lines = splitLines(str);
    var metadata = {};
    for (var line of lines) {
      var [key, value] = line.split("|");
      key = fromBase64(key);
      if (!key.endsWith(BinarySuffix))
        value = fromBase64(value);
      var existing = metadata[key];
      if (Array.isArray(existing))
        existing.push(value);
      else if (typeof existing == "string")
        metadata[key] = [existing, value];
      else
        metadata[key] = value;
    }
    return metadata;
  }
  var splitLines = function(str) {
    return str.split(/\r?\n/);
  };

  // src/GrpcWebConnector/Call.ts
  var Call = class {
    constructor(channel, aborter, callobj) {
      this.callobj = callobj;
      this.channel = channel;
      this.aborter = aborter;
    }
    cancel() {
      this.aborter.abort();
    }
  };

  // src/GrpcWebConnector/Channel.ts
  function CodeNum(str) {
    return GrpcStatusCode[str];
  }
  var _Channel = class {
    constructor(channelKey, address, instance) {
      this.callMap = new Map();
      this.channelKey = channelKey;
      this.transport = new GrpcWebFetchTransport({ baseUrl: address });
      this.instance = instance;
    }
    reportFinishedCall(callKey, status, trailers, message) {
      let params = [
        this.channelKey,
        callKey,
        CodeNum(status.code),
        toBase64(status.detail),
        toBase64(EncodeMetadata(trailers))
      ];
      if (message)
        params.push(encode(message.bytes.buffer));
      this.instance.unityCaller.OnCallCompletion(params.join("|"));
    }
    unaryRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
      const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.Unary);
      const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
      const aborter = new AbortController();
      let options = {
        abort: aborter.signal,
        meta: DecodeMetadata(headers),
        timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1e3) : void 0
      };
      options = this.transport.mergeOptions(options);
      const call = this.transport.unary(requestMethod, request, options);
      const callObj = new Call(this, aborter, call);
      const callKey = _Channel.callCounter++;
      this.callMap.set(callKey, callObj);
      call.headers.then((it) => this.instance.unityCaller.OnHeaders([this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join("")));
      call.then((it) => this.reportFinishedCall(callKey, it.status, it.trailers, it.response), (it) => it instanceof RpcError ? this.reportFinishedCall(callKey, { code: it.code, detail: it.message }, it.meta) : this.reportFinishedCall(callKey, { code: GrpcStatusCode[GrpcStatusCode.INTERNAL], detail: "Internal error in Channel.ts" + it.message }, it.meta));
      return callKey;
    }
    serverStreamRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
      const requestMethod = makeUnityMethodInfo(serviceName, methodName, GrpcRequestType.ServerStreaming);
      const request = UnityProtoMessage.fromBinary(new Uint8Array(decode(base64Message)));
      const aborter = new AbortController();
      let options = {
        abort: aborter.signal,
        meta: DecodeMetadata(headers),
        timeout: deadlineTimestampSecs ? new Date(deadlineTimestampSecs * 1e3) : void 0
      };
      options = this.transport.mergeOptions(options);
      const call = this.transport.serverStreaming(requestMethod, request, options);
      const callObj = new Call(this, aborter, call);
      const callKey = _Channel.callCounter++;
      this.callMap.set(callKey, callObj);
      call.headers.then((it) => this.instance.unityCaller.OnHeaders([this.channelKey, "|", callKey, "\n", EncodeMetadata(it)].join("")));
      call.responses.onMessage((message) => {
        const encodedMessage = encode(message.bytes.buffer);
        this.instance.unityCaller.OnServerStreamingResponse([this.channelKey, callKey, encodedMessage].join("|"));
        return;
      });
      call.then((it) => this.reportFinishedCall(callKey, it.status, it.trailers), (it) => it instanceof RpcError ? this.reportFinishedCall(callKey, { code: it.code, detail: it.message }, it.meta) : this.reportFinishedCall(callKey, { code: GrpcStatusCode[GrpcStatusCode.INTERNAL], detail: "Internal error in Channel.ts" + it.message }, it.meta));
      return callKey;
    }
    cancelRequest(callKey) {
      const call = this.findCall(callKey);
      call.cancel();
    }
    findCall(callKey) {
      const call = this.callMap.get(callKey);
      if (!call)
        throw new Error(`Invalid callKey: ${callKey}`);
      return call;
    }
  };
  var Channel = _Channel;
  Channel.callCounter = 0;
  var GrpcRequestType;
  (function(GrpcRequestType2) {
    GrpcRequestType2[GrpcRequestType2["Unary"] = 0] = "Unary";
    GrpcRequestType2[GrpcRequestType2["ServerStreaming"] = 1] = "ServerStreaming";
    GrpcRequestType2[GrpcRequestType2["ClientStreaming"] = 2] = "ClientStreaming";
    GrpcRequestType2[GrpcRequestType2["Duplex"] = 3] = "Duplex";
  })(GrpcRequestType || (GrpcRequestType = {}));
  function makeUnityMethodInfo(serviceName, methodName, requestType) {
    function lastElement(arr) {
      return arr.length ? arr[arr.length - 1] : null;
    }
    return {
      service: {
        methods: [],
        options: {},
        typeName: serviceName
      },
      name: methodName,
      localName: lastElement(methodName.split(".")),
      idempotency: void 0,
      serverStreaming: [1, 3].includes(requestType),
      clientStreaming: [2, 3].includes(requestType),
      options: {},
      I: UnityProtoMessage,
      O: UnityProtoMessage
    };
  }

  // src/GrpcWebConnector/Instance.ts
  var _Instance = class {
    constructor(unityCaller, instanceKey) {
      this.channelMap = new Map();
      this.unityCaller = unityCaller;
      this.instanceKey = instanceKey;
    }
    makeChannel(address) {
      const channelKey = _Instance.channelCounter++;
      const channel = new Channel(channelKey, address, this);
      this.channelMap.set(channelKey, channel);
      return channelKey;
    }
    findChannel(channelKey) {
      const channel = this.channelMap.get(channelKey);
      if (!channel)
        throw new Error(`Invalid channelKey: ${channelKey}`);
      return channel;
    }
  };
  var Instance = _Instance;
  Instance.channelCounter = 0;

  // src/GrpcWebConnector/Delegator.ts
  var _Delegator = class {
    constructor() {
      this.instanceMap = new Map();
    }
    findInstance(instanceKey) {
      const instance = this.instanceMap.get(instanceKey);
      if (!instance)
        throw new Error(`Invalid instanceKey: ${instanceKey}`);
      return instance;
    }
    RegisterInstance(unityCaller, objectName) {
      const handler = {
        get(target, propName, _receiver) {
          if (propName == "Module")
            return target.Module;
          return function(param) {
            target.Module.SendMessage(objectName, propName.toString(), param);
          };
        }
      };
      const connector = new Proxy({ Module: unityCaller }, handler);
      _Delegator.instanceCounter++;
      const instance = new Instance(connector, _Delegator.instanceCounter);
      this.instanceMap.set(instance.instanceKey, instance);
      connector.OnInstanceRegistered(instance.instanceKey);
    }
    RegisterChannel(instanceKey, address) {
      const instance = this.findInstance(instanceKey);
      return instance.makeChannel(address);
    }
    UnaryRequest(instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
      const instance = this.findInstance(instanceKey);
      const channel = instance.findChannel(channelKey);
      return channel.unaryRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
    }
    ServerStreamingRequest(instanceKey, channelKey, serviceName, methodName, headers, base64Message, deadlineTimestampSecs) {
      const instance = this.findInstance(instanceKey);
      const channel = instance.findChannel(channelKey);
      return channel.serverStreamRequest(serviceName, methodName, headers, base64Message, deadlineTimestampSecs);
    }
    CancelCall(instanceKey, channelKey, callKey) {
      const instance = this.findInstance(instanceKey);
      const channel = instance.findChannel(channelKey);
      const call = channel.findCall(callKey);
      call.cancel();
    }
  };
  var Delegator = _Delegator;
  Delegator.instanceCounter = 0;

  // src/main.ts
  window.GrpcWebUnityDelegator = new Delegator();
})();
