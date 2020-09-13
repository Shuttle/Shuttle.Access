<template>
  <div>
    <s-title :text="$t('users')" />
    <b-form class="my-2" v-if="$access.hasPermission('access://users/manage')">
      <b-input-group>
        <b-input-group-prepend>
          <b-button variant="outline-primary" @click="add">
            <font-awesome-icon icon="plus-square" />
          </b-button>
        </b-input-group-prepend>
        <b-form-input v-model="form.username"></b-form-input>
      </b-input-group>
    </b-form>
    <b-table :items="users" :fields="fields" dark responsive="md">
      <template v-slot:cell(roles)="data">
        <b-button
          variant="outline-primary"
          @click="roles(data.item)"
          size="sm"
          :disabled="!$access.hasPermission('access://users/manage')"
        >
          <font-awesome-icon icon="user-circle" />
        </b-button>
      </template>
      <template v-slot:cell(remove)="data">
        <b-button
          variant="outline-danger"
          v-b-modal.modal-confirmation
          size="sm"
          @click="selectUser(data.item)"
          :disabled="!$access.hasPermission('access://users/manage')"
        >
          <font-awesome-icon icon="trash-alt" />
        </b-button>
      </template>
    </b-table>
    <b-modal
      id="modal-confirmation"
      :title="$t('confirmation-remove.title')"
      button-size="sm"
      :ok-title="$t('ok')"
      :cancel-title="$t('cancel')"
      @ok="remove"
    >
      <p>{{$t('confirmation-remove.message')}}</p>
    </b-modal>
  </div>
</template>

<script>
import { required } from "vuelidate/lib/validators";

export default {
  name: "Users",
  data() {
    return {
      users: Array,
      fields: Array,
      selectedRole: Object,
      form: {
        username: "",
      },
    };
  },
  validations: {
    form: {
      username: {
        required,
      },
    },
  },
  methods: {
    roles(data) {
      this.$router.push({ name: "user-roles", params: { id: data.id } });
    },
    refresh() {
      const self = this;

      this.$api.get("users").then(function (response) {
        if (!response || !response.data) {
          return;
        }

        self.users = response.data;
      });
    },
    add(evt) {
      const self = this;

      evt.preventDefault();

      if (this.$v.$invalid) {
        return;
      }

      this.$api
        .post("users", {
          username: this.form.username,
        })
        .then(function () {
          self.$store.dispatch("addAlert", {
            message: self.$i18n.t("request-sent"),
          });
        });
    },
    remove() {
      const self = this;

      this.$api.delete(`users/${this.selectedUser.id}`).then(function () {
        self.$store.dispatch("addAlert", {
          message: self.$i18n.t("request-sent"),
        });
      });
    },
    selectUser(item) {
      this.selectedUser = item;
    },
  },
  beforeMount() {
    const self = this;

    this.fields = [
      {
        label: "",
        key: "roles",
      },
      {
        label: "",
        key: "remove",
      },
      {
        label: this.$i18n.t("username"),
        key: "username",
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
