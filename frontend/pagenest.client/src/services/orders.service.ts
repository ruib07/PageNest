import apiRequest from './helpers/api.service';

const route = 'orders';

export const GetOrders = async () => apiRequest('GET', route, undefined, true);
