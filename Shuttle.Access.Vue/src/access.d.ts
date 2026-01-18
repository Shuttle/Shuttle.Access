export type Alert = {
  message: string;
  name: string;
  type?: "error" | "success" | "warning" | "info" | undefined;
  expire?: boolean;
  expirySeconds?: number;
  expiryDate?: Date;
  dismissable?: boolean;
  key?: string;
  variant?: string;
  visiblePercentage?: number;
};

export type Configuration = {
  isOk: () => boolean;
  getErrorMessage: () => string;
  getUrl: () => string;
  isPasswordAuthenticationAllowed: () => boolean;
  isDebugging: () => boolean;
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

export type Credentials = {
  identityName: string;
  token?: string;
  password?: string;
  applicationName?: string;
  tenantId?: string;
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
  description?: string;
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
  description: string;
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
  description: string;
  password: string;
  system: string;
};

export type RegisterPermission = {
  name: string;
  description: string;
  status: number;
};

export type ServerConfiguration = {
  allowPasswordAuthentication: boolean;
};

export type Session = {
  identityId: string;
  identityName: string;
  identityDescription: string;
  permissions: string[];
  expiryDate?: Date;
  dateRegistered?: Date;
};

export type SessionData = {
  identityId: string;
  identityName: string;
  identityDescription: string;
  permissions: Permission[];
  expiryDate?: Date;
  dateRegistered?: Date;
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
  tenantId: string;
};

export type Tenant = {
  id: string;
  name: string;
  status: number;
};
