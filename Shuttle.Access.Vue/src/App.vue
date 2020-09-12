<template>
  <div id="app">
    <sv-navigation />
    <s-alerts :alerts="alerts" v-on:removed="removeAlert" class="m-2" />
    <div class="container-fluid">
      <div class="row row-offcanvas row-offcanvas-left">
        <div id="application-content" class="main col-sm-12 col-md-12">
          <router-view></router-view>
        </div>
      </div>
    </div>
    <footer class="footer p-2">
      <div class="container-fluid text-center">
        <div class="d-none d-xl-block font-weight-bold">X-LARGE (XL)</div>
        <div class="d-none d-lg-block d-xl-none font-weight-bold">LARGE (LG)</div>
        <div class="d-none d-md-block d-lg-none font-weight-bold">MEDIUM (M)</div>
        <div class="d-none d-sm-block d-md-none font-weight-bold">SMALL (SM)</div>
        <div class="d-block d-sm-none alert font-weight-bold">X-SMALL (Default)</div>
        <p class="m-0">Copyright (c) 2020 Shuttle</p>
      </div>
    </footer>
  </div>
</template>

<script>
import "@/assets/bootstrap.scss";
import "@/assets/shuttle.css";
import configuration from "@/configuration";

import Navigation from "./components/sv-navigation.vue";

export default {
  name: "app",
  components: {
    "sv-navigation": Navigation
  },
  computed: {
    debugging() {
      return configuration.debugging();
    },
    alerts() {
      return this.$store.state.alerts.messages;
    },
  },
  methods: {
    removeAlert(alert) {
      this.$store.commit("REMOVE_ALERT", alert);
    },
  },

};
</script>
