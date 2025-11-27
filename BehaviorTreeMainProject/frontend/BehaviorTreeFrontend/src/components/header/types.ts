export interface HeaderProps {
  theme: "light" | "dark";
  onToggleTheme: () => void;
}

export interface DropdownProps {
  title: string;
  items: string[];
}
