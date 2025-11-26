import { useState, useEffect, useRef } from 'react';
import './Header.css';

interface DropdownProps {
  title: string;
  items: string[];
}

/**
 * Component for a dropdown menu in the header.
 * @param param0 
 * @returns The Dropdown component. 
 */
function Dropdown({ title, items }: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  /**
   * Closes the dropdown when clicking outside of it
   * @param event MouseEvent
   */
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div 
      className="dropdown"
      ref={dropdownRef}
    >
      <button 
        className="dropdown-trigger"
        onClick={() => setIsOpen(!isOpen)} 
      >
        {title}
      </button>
      
      {isOpen && (
        <div className="dropdown-menu">
          {items.map((item, index) => (
            <button 
              key={index} 
              className="dropdown-item"
              onClick={() => setIsOpen(false)} 
            >
              {item}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

export default function Header() {
  return (
    <header className="header">
      <div className="header-left">
        <nav className="header-nav">
          <Dropdown 
            title="File" 
            items={['New', 'Open...', 'Save', 'Save As...', 'Export', 'Import']} 
          />
          <Dropdown 
            title="Edit" 
            items={['Undo', 'Redo', 'Cut', 'Copy', 'Paste', 'Delete']} 
          />
          <Dropdown 
            title="View" 
            items={['Zoom In', 'Zoom Out', 'Reset Zoom', 'Toggle Grid']} 
          />
        </nav>

        <div className="header-separator"></div>

        <div className="header-actions">
          <button className="icon-btn" title="Undo">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 14L4 9l5-5"/>
              <path d="M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5v0a5.5 5.5 0 0 1-5.5 5.5H11"/>
            </svg>
          </button>
          
          <button className="icon-btn" title="Redo">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M15 14l5-5-5-5"/>
              <path d="M20 9H9.5A5.5 5.5 0 0 0 4 14.5v0A5.5 5.5 0 0 0 9.5 20H13"/>
            </svg>
          </button>
        </div>
      </div>
    </header>
  );
}