import { Outlet } from 'react-router-dom';
import { SidebarProvider, useSidebar } from '../context/SidebarContext';
import AppHeader from './AppHeader';
import AppSidebar from './AppSidebar';
import Backdrop from './Backdrop';
import { useAuth } from '../context/AuthContext';

function LayoutContent() {
  const { isExpanded, isHovered, isMobileOpen } = useSidebar();
  const { user } = useAuth();

  const shouldShowSidebar = user?.role === 0;

  const sidebarWidth =
    isExpanded || isHovered ? 'lg:ml-[290px]' : 'lg:ml-[90px]';

  return (
    <div className="min-h-screen">
      <div>
        {shouldShowSidebar && <AppSidebar />}
        {shouldShowSidebar && <Backdrop />}
      </div>
      <div
        className={`flex-1 transition-all duration-300 ease-in-out ${
          shouldShowSidebar ? sidebarWidth : ''
        } ${isMobileOpen ? 'ml-0' : ''}`}
      >
        <AppHeader />
        <div className="p-4 mx-auto max-w-(--breakpoint-2xl) md:p-6">
          <Outlet />
        </div>
      </div>
    </div>
  );
}

export default function AppLayout() {
  return (
    <SidebarProvider>
      <LayoutContent />
    </SidebarProvider>
  );
}
