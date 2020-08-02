<template>
  <b-navbar toggleable="lg" fixed="top">
    <b-navbar-brand href="#">
      <img src="@/assets/logo-small.png" alt="Shuttle.Access logo" />
    </b-navbar-brand>
    <b-navbar-toggle target="nav-text-collapse" v-if="authenticated"></b-navbar-toggle>
    <b-collapse id="nav-text-collapse" is-nav>
      <b-navbar-nav v-if="authenticated">
        <b-nav-item to="/roles/list">Roles</b-nav-item>
      </b-navbar-nav>
      <b-navbar-nav class="ml-auto">
        <b-nav-item-dropdown right v-if="authenticated">
          <template v-slot:button-content>
            <font-awesome-icon icon="user" />
          </template>
          <b-dropdown-item to="/profile">Profile</b-dropdown-item>
          <b-dropdown-item href="#" v-on:click="logout">Log out</b-dropdown-item>
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
