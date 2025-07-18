import type { ILanguage } from '../types/language';
import apiRequest from './helpers/api.service';

const route = 'languages';

export const GetLanguages = async () =>
  apiRequest('GET', route, undefined, true);

export const GetLanguageById = async (languageId: ILanguage['id']) =>
  apiRequest('GET', `${route}/${languageId}`, undefined, true);
