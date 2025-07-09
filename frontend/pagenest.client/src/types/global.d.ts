export interface IOption {
  value: string | number;
  label: string;
}

export type INavItem = {
  name: string;
  icon: React.ReactNode;
  path?: string;
  subItems?: { name: string; path: string; new?: boolean }[];
};
