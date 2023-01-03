"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var BackOfficeService = /** @class */ (function () {
    function BackOfficeService(http) {
        this.http = http;
    }
    BackOfficeService.prototype.adjustPlayerLockedAmount = function (playerCode, lockAmount, remark) {
        return this.http.put("/api/player/wallet/locked/".concat(playerCode), { lockAmount: lockAmount, remark: remark });
    };
    return BackOfficeService;
}());
exports.default = BackOfficeService;
