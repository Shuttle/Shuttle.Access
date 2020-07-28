<template>
  <div v-if="alerts" class="m-2">
    <b-alert
      v-for="alert in alerts"
      :key="alert.name"
      :variant="alert.type"
      show
      class="alert-dismissible"
    >
      <button
        type="button"
        class="close"
        aria-label="Close"
        data-dismiss="alert"
        v-on:click="remove(alert)"
      >
        <span aria-hidden="true">&times;</span>
      </button>
      {{alert.message}}
    </b-alert>
  </div>
</template>

<script>
export default {
  methods: {
    remove(alert) {
      this.$store.commit("removeAlert", alert);
    }
  },
  computed: {
    alerts() {
      return this.$store.state.alerts.messages;
    }
  }
};
</script>