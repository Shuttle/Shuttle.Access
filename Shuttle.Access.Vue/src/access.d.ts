export type Alert = {
  message: string;
  name: string;
  type?: "error" | "success" | "warning" | "info" | undefined;
  expire?: boolean;
  expirySeconds?: number;
  dismissable?: boolean;
  key?: string;
  variant?: string;
};

export type AlertStoreState = {
  alerts: Alert[];
};

export type Configuration = {
  url: string;
  allowPasswordAuthentication: boolean;
  debugging: () => boolean;
  getApiUrl: (path: string) => string;
};

export type ChangePassword = {
  id: string;
  token: string;
  newPassword: string;
};

export type ConfirmationOptions = {
  item?: any;
  onConfirm: (item?: any) => void;
  message?: string;
  title?: string;
};

export type ConfirmationStoreState = {
  isOpen: boolean;
  options?: ConfirmationOptions;
};

export type Credentials = {
  identityName: string;
  token?: string;
  password?: string;
  applicationName?: string;
};

export type DashboardItem = {
  route: string;
  title: string;
  value: number;
};

export type DrawerOptions = {
  parentPath: string;
  refresh: () => Promise<void>;
};

export type Env = {
  VITE_API_URL: string;
  VITE_API_ALLOW_PASSWORD_AUTHENTICATION: string;
};

export type FormDrawer = {
  closePath: string;
};

export type FormTitle = {
  title: string;
  closeDrawer?: boolean;
  closePath?: string;
  closeClick?: () => void;
  type?: "borderless" | "normal";
};

export type IdentifierAvailability = {
  id: string;
  active: boolean;
};

export type Identity = {
  dateActivated?: Date | null;
  dateRegistered: Date;
  generatedPassword: string;
  id: string;
  name: string;
  registeredBy: string;
  roles: Role[] | undefined;
};

export type NavigationItem = {
  permission?: string;
  title: string;
  to: string;
};

export type OAuthData = {
  code: string;
  state: string;
};

export type Permission = {
  id: string;
  name: string;
  status: number;
};

export type Status = {
  text: string;
  value: number;
};

export type Role = {
  id: string;
  name: string;
  permissions?: Permission[];
};

export type RegisterIdentity = {
  name: string;
  password: string;
  system: string;
};

export type RegisterPermission = {
  name: string;
  status: number;
};

export type Session = {
  identityId: string;
  identityName: string;
  token: string;
  permissions: string[];
  expiryDate?: Date;
  dateRegistered?: Date;
};

export type SessionPermission = {
  type: string;
  permission: string;
};

export type SessionResponse = {
  identityId: string;
  identityName: string;
  permissions: string[];
  registrationRequested: boolean;
  result: string;
  token: string;
  tokenExpiryDate: string;
  sessionTokenExchangeUrl?: string;
};

export type SessionStoreState = {
  authenticated: boolean;
  initialized: boolean;
  identityName?: string;
  token?: string;
  permissions: SessionPermission[];
};

export type SnackbarStoreState = {
  visible: boolean;
  text: string;
  timeout: number;
};
