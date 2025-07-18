import apiRequest from './helpers/api.service';

const route = 'genres';

export const GetGenres = async () => apiRequest('GET', route, undefined, true);
