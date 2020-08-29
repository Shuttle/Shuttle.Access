<template>
  <div>
    <b-navbar toggleable="lg" fixed="top" type="dark" class="navbar-primary">
      <b-navbar-brand to="dashboard">
        <img src="@/assets/logo-small.png" alt="Shuttle.Access logo" />
      </b-navbar-brand>
      <b-navbar-toggle target="nav-text-collapse" v-if="authenticated"></b-navbar-toggle>
      <b-collapse id="nav-text-collapse" is-nav v-if="authenticated">
        <b-navbar-nav>
          <b-nav-item to="/roles">{{$t("roles")}}</b-nav-item>
        </b-navbar-nav>
        <b-navbar-nav>
          <b-nav-item to="/users">{{$t("users")}}</b-nav-item>
        </b-navbar-nav>
        <b-navbar-nav class="ml-auto" right v-if="authenticated">
          <b-nav-item to="profile">
            <font-awesome-icon icon="user" />
          </b-nav-item>
          <b-nav-item v-on:click="logout">
            <font-awesome-icon icon="sign-out-alt" />
          </b-nav-item>
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>
    <b-navbar type="dark" class="navbar-secondary" v-if="hasSecondaryNavbarItems">
      <b-nav-form>
        <b-button v-for="item in secondaryNavbarItems" :key="item.key" :variant="!!item.variant ? item.variant : 'outline-secondary'" class="mr-2" @click="item.click"><font-awesome-icon v-if="!!item.icon" :icon="item.icon" /></b-button>
      </b-nav-form>
    </b-navbar>
  </div>
</template>

<script>
export default {
  computed: {
    authenticated() {
      return this.$store.getters.authenticated;
    },
    hasSecondaryNavbarItems() {
      return this.secondaryNavbarItems.length > 0;
    },
    secondaryNavbarItems() {
      return this.$store.getters.secondaryNavbarItems;
    },
  },
  methods: {
    logout() {
      this.$store.dispatch("logout");
    },
  },
};
</script>
