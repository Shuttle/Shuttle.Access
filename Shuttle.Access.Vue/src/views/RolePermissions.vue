<template>
  <div>
    <s-title :text="title" />
    <b-table :items="permissions" :fields="fields" dark responsive="md">
      <template v-slot:cell(active)="data">
        <font-awesome-icon v-if="data.item.working" icon="hourglass" />
        <b-form-checkbox v-else v-model="data.item.active" switch @input="toggle(data.item)" />
      </template>
      <template v-slot:cell(permission)="data">
        <span :class="!data.item.active ? 'text-muted' : ''">{{data.item.permission}}</span>
      </template>
    </b-table>
  </div>
</template>

<script>
export default {
  name: "RolePermissions",
  data() {
    return {
      roleId: "",
      roleName: "",
      rolePermissions: [],
      availablePermissions: [],
      permissions: [],
      fields: [],
      working: false,
    };
  },
  computed: {
    title() {
      return (
        this.$i18n.t("role-permissions") +
        (this.roleName ? " - " + this.roleName : "")
      );
    },
    workingItems() {
      return this.permissions.filter(function (item) {
        return item.working;
      });
    },
    workingCount() {
      return this.workingItems.length;
    },
  },
  methods: {
    toggle(item) {
      var self = this;

      if (this.working) {
        self.$store.dispatch("addAlert", {
          message: self.$i18n.t("working-message"),
          name: "working-message",
        });
        return;
      }

      item.working = true;

      if (this.workingCount === 1) {
        this.getPermissionStatus();
      }

      this.$api
        .post("roles/setpermission", {
          roleId: this.roleId,
          permission: item.permission,
          active: item.active,
        })
        .finally(function () {
          self.working = false;
        });
    },

    getPermissionItem(permission) {
      var result;

      Array.prototype.forEach.call(this.permissions, (item) => {
        if (result) {
          return;
        }

        if (item.permission === permission) {
          result = item;
        }
      });

      return result;
    },

    getPermissionStatus() {
      var self = this;

      if (this.workingCount === 0) {
        return;
      }

      var data = {
        roleId: self.roleId,
        permissions: [],
      };

      Array.prototype.forEach.call(this.workingItems, (item) => {
        data.permissions.push(item.permission);
      });

      this.$api
        .post("roles/permissionStatus", data)
        .then(function (response) {
          Array.prototype.forEach.call(response.data, (item) => {
            const permissionItem = self.getPermissionItem(item.permission);

            if (!permissionItem) {
              return;
            }

            permissionItem.working = !(permissionItem.active === item.active);
          });
        })
        .then(function () {
          setTimeout(function () {
            self.getPermissionStatus.call(self);
          }, 1000);
        });
    },

    applyPermissions() {
      var permissions = [];

      Array.prototype.forEach.call(this.rolePermissions, (item) => {
        permissions.push({
          permission: item.permission,
          active: true,
          working: false,
        });
      });
      Array.prototype.forEach.call(
        this.availablePermissions.filter((item) => {
          return !permissions.some((p) => p.permission == item.permission);
        }),
        (item) => {
          permissions.push({
            permission: item.permission,
            active: false,
            working: false,
          });
        }
      );

      this.permissions = permissions;
    },
    refresh() {
      const self = this;

      this.$api.get("roles/" + this.roleId).then(function (response) {
        self.roleName = response.data.roleName;
        self.rolePermissions = response.data.permissions;

        self.$api.get("permissions").then(function (response) {
          self.availablePermissions = response.data;
          self.applyPermissions();
        });
      });
    },
  },
  beforeMount() {
    this.roleId = this.$route.params.id;

    this.fields = [
      {
        label: "",
        key: "active",
        class: "text-center"
      },
      {
        key: "permission",
        label: this.$i18n.t("permission"),
        class: "col",
      },
    ];
  },
  mounted() {
    this.refresh();
  },
};
</script>