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
      { name: 'Courses', path: '/admin/courses' },
      { name: 'Players', path: '/admin/players' },
      { name: 'Users', path: '/admin/users' },
    ],
  },
];
