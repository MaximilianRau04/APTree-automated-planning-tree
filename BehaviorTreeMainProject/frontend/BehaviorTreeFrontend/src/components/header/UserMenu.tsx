import { useState, useEffect, useRef } from "react";

export function UserMenu() {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div className="user-menu" ref={menuRef}>
      <button 
        className="icon-btn settings-btn"
        title="Settings"
        aria-label="Settings"
        onClick={() => setIsOpen(!isOpen)}
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="12" cy="12" r="3" />
          <path d="M12 1v6m0 6v6m5.2-14.8l-4.2 4.2m0 5.2l4.2 4.2M23 12h-6m-6 0H1m14.8 5.2l-4.2-4.2m0-5.2l-4.2-4.2" />
        </svg>
      </button>

      <button 
        className="profile-btn"
        title="Profile"
        aria-label="Profile"
        onClick={() => setIsOpen(!isOpen)}
      >
        <svg
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
          <circle cx="12" cy="7" r="4" />
        </svg>
      </button>

      {isOpen && (
        <div className="user-dropdown">
          <button className="dropdown-item">Profile Settings</button>
          <button className="dropdown-item">Preferences</button>
          <button className="dropdown-item">Help</button>
          <div className="dropdown-divider"></div>
          <button className="dropdown-item">Logout</button>
        </div>
      )}
    </div>
  );
}