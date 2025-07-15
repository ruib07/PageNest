import apiRequest from './helpers/api.service';

const route = 'payments';

export const GetPayments = async () => apiRequest('GET', route, undefined, true);