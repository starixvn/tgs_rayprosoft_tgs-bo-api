"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var BackOfficeService = /** @class */ (function () {
    function BackOfficeService(http) {
        this.http = http;
    }
    BackOfficeService.prototype.adjustPlayerLockedAmount = function (playerCode, lockAmount, remark) {
        return this.http.put("/api/player/wallet/locked/".concat(playerCode), { lockAmount: lockAmount, remark: remark });
    };
    BackOfficeService.prototype.getPlayerIpsByUserName = function (brandId, userName, page, rows, sortby, orderby, showAll) {
        return this
            .http
            .get("/api/playerip/username", {
            params: {
                brandId: brandId,
                userName: userName,
                page: page,
                rows: rows,
                sortby: sortby,
                orderby: orderby,
                showAll: showAll
            }
        });
    };
    return BackOfficeService;
}());
exports.default = BackOfficeService;
