import type { ICategory } from '../types/category';
import apiRequest from './helpers/api.service';

const route = 'categories';

export const GetCategories = async () =>
  apiRequest('GET', route, undefined, true);

export const GetCategoryById = async (categoryId: ICategory['id']) =>
  apiRequest('GET', `${route}/${categoryId}`, undefined, true);
