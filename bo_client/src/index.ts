import axiosConfigFactory from './models/axios-configuration.js';
import BackOfficeService from './services/bo-service.js';

let instance = axiosConfigFactory("1bf65741-a8e1-4056-856d-9165c1a2094b", true);

let svc = new BackOfficeService(instance);

svc.getPlayerPaymentOverview("test1")
    .then(result => console.log(result));
