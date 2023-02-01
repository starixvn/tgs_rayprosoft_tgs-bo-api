"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
Object.defineProperty(exports, "__esModule", { value: true });
var BackOfficeService = /** @class */ (function () {
    function BackOfficeService(http) {
        this.http = http;
    }
    BackOfficeService.prototype.adjustPlayerLockedAmount = function (playerCode, lockAmount, remark) {
        return this.http
            .put("/api/player/wallet/locked/".concat(playerCode), { lockAmount: lockAmount, remark: remark })
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.getPlayerIpsByUserName = function (brandId, userName, page, rows, sortby, orderby, showAll) {
        return this.http
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
        })
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.getPlayerPaymentOverview = function (playerCode) {
        return this.http
            .get("/api/player/payment/".concat(playerCode), {})
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.increasePlayerAmount = function (playerCode, request) {
        return this.http
            .post("/api/player/wallet/increase/".concat(playerCode), request)
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.reducePlayerAmount = function (playerCode, request) {
        return this.http
            .post("/api/player/wallet/reduce/".concat(playerCode), request)
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.listAgentPlayerWinloss = function (agentId, condition, paging) {
        return this.http
            .get("/api/agentreport/".concat(agentId, "/winloss"), {
            params: __assign(__assign({}, condition), paging)
        })
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.addAgentDomain = function (brandId, agentId, domain) {
        return this.http
            .put("/api/agent/domain/".concat(brandId, "/").concat(agentId), null, {
            params: {
                domain: domain
            }
        })
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.listAgentDomains = function (brandId, agentId) {
        return this.http
            .get("/api/agent/domain/".concat(brandId, "/").concat(agentId))
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.removeAgentDomain = function (brandId, agentId, domain) {
        return this.http
            .delete("/api/agent/domain/".concat(brandId, "/").concat(agentId), {
            params: {
                domain: domain
            }
        })
            .then(function (res) { return res.data; });
    };
    BackOfficeService.prototype.getVipAdjustInfos = function (playerCode, brandId, from, to, page, rows, sortby, orderby, showAll) {
        return this
            .http
            .get("/api/player/vipAdjustInfos/".concat(playerCode), {
            params: {
                brandId: brandId,
                from: from,
                to: to,
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
