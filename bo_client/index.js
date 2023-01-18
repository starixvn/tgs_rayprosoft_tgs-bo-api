"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
var axios_configuration_js_1 = __importDefault(require("./models/axios-configuration.js"));
var bo_service_js_1 = __importDefault(require("./services/bo-service.js"));
var instance = (0, axios_configuration_js_1.default)("1bf65741-a8e1-4056-856d-9165c1a2094b", true);
var svc = new bo_service_js_1.default(instance);
svc.getPlayerPaymentOverview("test1")
    .then(function (result) { return console.log(result); });
