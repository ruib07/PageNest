import { AdminIcon, GridIcon } from '../icons';
import type { INavItem } from '../types/global';

export const navItems: INavItem[] = [
  {
    icon: <GridIcon />,
    name: 'Home',
    path: '/',
  },
  {
    name: 'Admin',
    icon: <AdminIcon />,
    subItems: [
      { name: 'Categories', path: '/admin/categories' },
      { name: 'Genres', path: '/admin/genres' },
      { name: 'Languages', path: '/admin/languages' },
      { name: 'Books', path: '/admin/books' },
      { name: 'Orders', path: '/admin/orders' },
      { name: 'Payments', path: '/admin/payments' },
    ],
  },
];
