export type IdentifierAvailability = {
  id: string;
  active: boolean;
};

export type RoleAssignment = {
  roleId: string;
  roleName: string;
  active: boolean;
  activeOnToggle: boolean;
  working: boolean;
};
