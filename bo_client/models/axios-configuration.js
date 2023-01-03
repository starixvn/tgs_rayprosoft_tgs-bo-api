"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
var axios_1 = __importDefault(require("axios"));
function axiosConfigFactory(name, isWindows) {
    if (isWindows)
        return axios_1.default.create({
            baseURL: "http://localhost",
            socketPath: "\\\\.\\pipe\\".concat(name),
            headers: {
                common: {
                    "Host": "localhost"
                }
            },
            insecureHTTPParser: true
        });
    else
        return axios_1.default.create({
            baseURL: "http://localhost",
            socketPath: "/tmp/".concat(name, ".sock"),
            headers: {
                common: {
                    "Host": "localhost",
                    "Accept-Encoding": ["gzip", "compress", "deflate"]
                }
            }
        });
}
exports.default = axiosConfigFactory;
