import axios from "axios";

export default function axiosConfigFactory(name: string, isWindows: boolean) {
    if (isWindows)
        return axios.create({
            baseURL: "http://localhost",
            socketPath: `\\\\.\\pipe\\${name}`,
            headers: {
                common: {
                    "Host": "localhost"
                }
            },
            insecureHTTPParser: true
        });
    else
        return axios.create({
            baseURL: "http://localhost",
            socketPath: `/tmp/${name}.sock`,
            headers: {
                common: {
                    "Host": "localhost",
                    "Accept-Encoding": ["gzip", "compress", "deflate"]
                }
            }
        });
} 