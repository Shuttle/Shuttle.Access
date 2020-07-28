<template>
  <div id="app">
    <sa-navigation />
    <sa-alerts />
    <div class="container-fluid">
      <div class="row row-offcanvas row-offcanvas-left">
        <div id="application-content" class="main col-sm-12 col-md-12">
          <router-view></router-view>
        </div>
      </div>
    </div>
    <footer class="footer text-muted mt-2">
      <div class="container-fluid p-3 text-center">
        <p class="m-0">Copyright (c) 2020 Shuttle</p>
      </div>
    </footer>
    <div class="alert alert-dark" v-if="debugging">
      <div class="d-none d-xl-block font-weight-bold">X-LARGE (XL)</div>
      <div class="d-none d-lg-block d-xl-none font-weight-bold">LARGE (LG)</div>
      <div class="d-none d-md-block d-lg-none font-weight-bold">MEDIUM (M)</div>
      <div class="d-none d-sm-block d-md-none font-weight-bold">SMALL (SM)</div>
      <div class="d-block d-sm-none alert font-weight-bold">X-SMALL (Default)</div>
    </div>
  </div>
</template>

<script>
import "@/assets/bootstrap.scss";
import "@/assets/main.css";
import configuration from "@/configuration";

import Navigation from "./components/sa-navigation.vue";
import Alerts from "./components/sa-alerts.vue";

export default {
  name: "app",
  components: {
    "sa-navigation": Navigation,
    "sa-alerts": Alerts
  },
  computed: {
    debugging() {
      return configuration.debugging();
    }
  }
};
</script>

<style lang="scss">
@import "../node_modules/ag-grid-community/dist/styles/ag-grid.css";
@import "../node_modules/ag-grid-community/dist/styles/ag-theme-balham.css";
</style>