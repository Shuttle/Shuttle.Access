<template>
  <div>
    <s-title :text="title" />
    :: {{roleId}}
  </div>
</template>

<script>
export default {
  name: "RolePermissions",
  data() {
    return {
        roleId: '',
        roleName: ''
    };
  },
  computed: {
      title(){
          return this.$i18n.t('role-permissions') + (this.roleName ? ' - ' + this.roleName : '');
      }
  },
  beforeMount() {
    this.roleId = this.$route.params.id;
  },
  mounted(){
      
  }
};
</script>