<template>
  <b-navbar toggleable="lg" fixed="top" type="dark">
    <b-navbar-brand to="dashboard">
      <img src="@/assets/logo-small.png" alt="Shuttle.Access logo" />
    </b-navbar-brand>
    <b-navbar-toggle target="nav-text-collapse" v-if="authenticated"></b-navbar-toggle>
    <b-collapse id="nav-text-collapse" is-nav v-if="authenticated">
      <b-navbar-nav>
        <b-nav-item to="roles">{{$t("roles")}}</b-nav-item>
      </b-navbar-nav>
      <b-navbar-nav>
        <b-nav-item to="users">{{$t("users")}}</b-nav-item>
      </b-navbar-nav>
      <b-navbar-nav class="ml-auto">
        <b-nav-item-dropdown right v-if="authenticated">
          <template v-slot:button-content>
            <font-awesome-icon icon="user" />
          </template>
          <b-dropdown-item to="profile">{{$t("profile")}}</b-dropdown-item>
          <b-dropdown-item href="#" v-on:click="logout">{{$t("log-out")}}</b-dropdown-item>
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
</template>

<script>
export default {
  computed: {
    authenticated() {
      return this.$store.getters.authenticated;
    }
  },
  methods: {
    logout() {
      this.$store.dispatch("logout");
    }
  }
};
</script>
