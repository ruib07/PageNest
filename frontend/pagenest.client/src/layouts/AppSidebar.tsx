import { useCallback } from 'react';
import { Link, useLocation } from 'react-router';
import { useAuth } from '../context/AuthContext';
import { useSidebar } from '../context/SidebarContext';
import { AdminIcon, GridIcon, HorizontaLDots } from '../icons';
import type { INavItem } from '../types/navItem';

export default function AppSidebar() {
  const { isExpanded, isMobileOpen, isHovered, setIsHovered } = useSidebar();
  const { user } = useAuth();
  const isAuthenticated = !!user;
  const location = useLocation();

  const isActive = useCallback(
    (path: string) => location.pathname === path,
    [location.pathname]
  );

  const getNavItemsByRole = (): INavItem[] => {
    if (!user) return [];

    return [
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
  };

  const renderMenuItems = (items: INavItem[]) => (
    <ul className="flex flex-col gap-4">
      {items.map((nav) =>
        nav.subItems ? (
          <li key={nav.name}>
            <div className="menu-item group text-gray-700 dark:text-white">
              <span className="menu-item-icon-size">{nav.icon}</span>
              {(isExpanded || isHovered || isMobileOpen) && (
                <span className="menu-item-text">{nav.name}</span>
              )}
            </div>
            <ul className="ml-6 mt-2 space-y-1">
              {nav.subItems.map((sub) => (
                <li key={sub.name}>
                  <Link
                    to={sub.path}
                    className={`menu-item group ${isActive(sub.path) ? 'menu-item-active' : 'menu-item-inactive'}`}
                  >
                    <span className="menu-item-text">{sub.name}</span>
                  </Link>
                </li>
              ))}
            </ul>
          </li>
        ) : (
          <li key={nav.name}>
            <Link
              to={nav.path!}
              className={`menu-item group ${isActive(nav.path!) ? 'menu-item-active' : 'menu-item-inactive'}`}
            >
              <span className="menu-item-icon-size">{nav.icon}</span>
              {(isExpanded || isHovered || isMobileOpen) && (
                <span className="menu-item-text">{nav.name}</span>
              )}
            </Link>
          </li>
        )
      )}
    </ul>
  );

  return (
    <aside
      className={`fixed mt-16 flex flex-col lg:mt-0 top-0 px-5 left-0 bg-white dark:bg-gray-900 dark:border-gray-800 text-gray-900 h-screen transition-all duration-300 ease-in-out z-50 border-r border-gray-200 
        ${
          isExpanded || isMobileOpen
            ? 'w-[290px]'
            : isHovered
              ? 'w-[290px]'
              : 'w-[90px]'
        }
        ${isMobileOpen ? 'translate-x-0' : '-translate-x-full'}
        lg:translate-x-0`}
      onMouseEnter={() => !isExpanded && setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div
        className={`py-8 flex ${
          !isExpanded && !isHovered ? 'lg:justify-center' : 'justify-start'
        }`}
      >
        <Link to="/">
          {isExpanded || isHovered || isMobileOpen ? (
            <>
              <img
                className="dark:hidden"
                src="/images/logo/logo.png"
                alt="Logo"
                width={160}
                height={50}
              />
              <img
                className="hidden dark:block"
                src="/images/logo/logo-dark.png"
                alt="Logo"
                width={160}
                height={50}
              />
            </>
          ) : (
            <img
              src="/images/logo/logo-icon.png"
              alt="Logo"
              width={32}
              height={32}
            />
          )}
        </Link>
      </div>
      <div className="flex flex-col overflow-y-auto duration-300 ease-linear no-scrollbar">
        <nav className="mb-6">
          <div className="flex flex-col gap-4">
            {isAuthenticated && (
              <div>
                <h2
                  className={`mb-4 text-xs uppercase flex leading-[20px] text-gray-400 ${
                    !isExpanded && !isHovered
                      ? 'lg:justify-center'
                      : 'justify-start'
                  }`}
                >
                  {isExpanded || isHovered || isMobileOpen ? (
                    'Menu'
                  ) : (
                    <HorizontaLDots className="size-6" />
                  )}
                </h2>
                {renderMenuItems(getNavItemsByRole())}
              </div>
            )}
          </div>
        </nav>
      </div>
    </aside>
  );
}
