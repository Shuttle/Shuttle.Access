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
  debugging: () => boolean;
  getApiUrl: (path: string) => string;
};

export type ChangePassword = {
  id: string;
  token: string;
  newPassword: string;
};

export type ConfirmationStoreState = {
  item: any;
  isOpen: boolean;
  callback?: (item: any) => void;
};

export type Credentials = {
  identityName: string;
  token?: string;
  password?: string;
};

export type DashboardItem = {
  route: string;
  title: string;
  value: number;
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

export type OAuthData = {
  code: string;
  state: string;
};

export type Permission = {
  id: string;
  name: string;
  status: number;
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
  identityName: string;
  token: string;
  permissions: string[];
};

export type SessionPermission = {
  type: string;
  permission: string;
};

export type SessionStoreState = {
  authenticated: boolean;
  initialized: boolean;
  identityName?: string;
  token?: string;
  permissions: SessionPermission[];
};
