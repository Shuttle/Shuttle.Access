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

export type ConfirmationItem = {
  name: string;
  getConfirmationMessage: () => string;
  touched?: boolean;
  unwatch?: WatchHandle;
};

export type ConfirmationOptions = {
  item?: any;
  messageKey?: string;
  messageText?: string;
  titleKey?: string;
  titleText?: string;
};

export type ConfirmationResult = {
  confirmed: boolean;
  item?: any;
};

export type Credentials = {
  identityName: string;
  token?: string;
  password?: string;
  tenantId?: string;
};

export type DashboardItem = {
  route: string;
  title: string;
  value: number;
  svg: string;
};

export type DrawerOptions = {
  parentPath: string;
  refresh: () => Promise<void>;
};

export type DrawerSize = "compact" | "expanded" | "full" | undefined;

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
  roles: IdentityRole[] | undefined;
  tenants: IdentityTenant[] | undefined;
};

export type IdentityRole = {
  id: string;
  name: string;
};

export type IdentityTenant = {
  id: string;
  name: string;
};

export type NavigationItem = {
  permission?: string;
  title: string;
  to: string;
  icon?: string;
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
  id: string;
  identityId: string;
  identityName: string;
  identityDescription: string;
  permissions: Permission[];
  expiryDate?: Date;
  dateRegistered?: Date;
  tenantId?: string;
  tenantName?: string;
  tokenHash?: number[];
};

export type SessionResponse = {
  session?: Session;
  registrationRequested: boolean;
  result: string;
  token: string | null;
  tenants: Tenant[];
};

export type Tenant = {
  id: string;
  name: string;
  logoSvg?: string;
  logoUrl?: string;
  status?: number;
  administratorIdentityName?: string;
};
