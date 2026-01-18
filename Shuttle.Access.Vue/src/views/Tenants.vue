<template>
  <v-card flat>
    <v-card-title class="sv-card-title">
      <sv-title :title="$t('tenants')" />
    </v-card-title>
    <v-divider></v-divider>
    <v-card-text>
      <!-- Tenants content will go here -->
      <p>Tenants view placeholder</p>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n({ useScope: 'global' });
</script>
