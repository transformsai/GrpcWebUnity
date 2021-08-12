const path = require("path");
const fs = require("fs");
const glob = require("glob");
const { exec } = require("child_process");

const PROTO_DIR = path.join("..", "Proto");
const PROTO_DIR_ABS = path.resolve(PROTO_DIR);
const PROTO_NAMESPACE = "grpcwebunity";
const PROTO_DIR_GLOB = path.join(PROTO_DIR, PROTO_NAMESPACE, "**", "*.proto");
const PROTO_OUTPUT_DIR = path.join(".", "src", "generated");
const PROTO_OUTPUT_DIR_GLOB = path.join(PROTO_OUTPUT_DIR, "**", "*");

const globOptions = {
  absolute: true,
};

function compileProtos() {
  const files = glob.sync(PROTO_DIR_GLOB, globOptions)
  console.info("Compiling Protos:", files);
  const fileList = files.map(f => `"${f}"`).join(" ");
  const cmd = `npx protoc --ts_out "${PROTO_OUTPUT_DIR}" --ts_opt generate_dependencies --proto_path "${PROTO_DIR_ABS}" ${fileList}`;
  fs.mkdirSync(path.join(PROTO_OUTPUT_DIR, PROTO_NAMESPACE), { recursive: true });
  exec(cmd, (error, stdout, stderr) => {
    if (error) throw error;
    if (stderr) throw new Error(stderr);
    if (stdout) {
      console.log(`protoc:\n${stdout}`);
    }
  });
  return glob.sync(PROTO_OUTPUT_DIR_GLOB, globOptions);
}

compileProtos();
