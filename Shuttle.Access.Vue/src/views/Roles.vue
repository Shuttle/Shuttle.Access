<template>
  <div>
    <s-title :text="$t('roles')" />
    <b-table :items="roles" :fields="fields" dark responsive="md">
      <template v-slot:cell(permissions)="data">
        <b-button
          variant="outline-primary"
          @click="permissions(data.item)"
          size="sm"
        >{{$t("permissions-button")}}</b-button>
      </template>
    </b-table>
  </div>
</template>

<script>
export default {
  name: "Roles",
  data() {
    return {
      roles: Array,
      fields: Array,
    };
  },
  methods: {
    permissions(data) {
      console.log(data.id);
    },
    refresh() {
      const self = this;

      this.$api.get("roles").then(function (response) {
        self.roles = response.data;
      });
    },
  },
  beforeMount() {
    const self = this;

    this.fields = [
      {
        label: this.$i18n.t("permissions"),
        key: "permissions",
      },
      {
        label: this.$i18n.t("role-name"),
        key: "roleName",
        thClass: "col",
      },
    ];

    this.$store.dispatch("addSecondaryNavbarItem", {
      icon: "sync-alt",
      click() {
        self.refresh();
      },
    });
  },
  mounted() {
    this.refresh();
  },
};
</script>
