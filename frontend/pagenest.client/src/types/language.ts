export interface ILanguage {
  id?: string;
  name: string;
  code: string;
  cultureCode: string;
}

export interface ILanguageRowProps {
  language: ILanguage;
}
