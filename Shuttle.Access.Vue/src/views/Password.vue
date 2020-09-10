<template>
  <div class="container-fluid">
    <div class="row">
      <div class="col"></div>
      <div class="col-xl-3 col-lg-4 col-md-5 mx-auto p-0 mt-5 mb-5">
        <s-title :text="$t('password')" />
        <b-form @submit="submit" v-if="show" class="w-100">
          <b-form-group
            id="input-group-old-password"
            :label="$t('old-password')"
            label-for="input-old-password"
            :class="this.token ? 'd-none' : ''"
          >
            <b-input-group>
              <b-form-input
                id="input-old-password"
                v-model="form.oldPassword"
                required
                :type="form.oldPasswordType"
              ></b-form-input>
              <b-input-group-append>
                <b-button @click="toggleOldPasswordView">
                  <font-awesome-icon :icon="passwordView(form.oldPasswordType)" />
                </b-button>
              </b-input-group-append>
            </b-input-group>
          </b-form-group>

          <b-form-group
            id="input-group-new-password"
            :label="$t('new-password')"
            label-for="input-new-password"
          >
            <b-input-group>
              <b-form-input
                id="input-new-password"
                v-model="form.newPassword"
                required
                :type="form.newPasswordType"
              ></b-form-input>
              <b-input-group-append>
                <b-button @click="toggleNewPasswordView">
                  <font-awesome-icon :icon="passwordView(form.newPasswordType)" />
                </b-button>
              </b-input-group-append>
            </b-input-group>
          </b-form-group>

          <b-form-group
            id="input-group-new-password-confirm"
            :label="$t('new-password-confirm')"
            label-for="input-new-password-confirm"
          >
            <b-input-group>
              <b-form-input
                id="input-new-password-confirm"
                v-model="form.newPasswordConfirm"
                required
                :type="form.newPasswordType"
              ></b-form-input>
            </b-input-group>
          </b-form-group>

          <div>
            <b-button class="float-right" variant="primary" type="submit" :disabled="working">
              <font-awesome-icon icon="circle-notch" class="fa-spin mr-2" v-if="working" />
              {{$t("submit")}}
            </b-button>
          </div>
        </b-form>
      </div>
      <div class="col"></div>
    </div>
  </div>
</template>

<script>
import { required } from "vuelidate/lib/validators";

export default {
  data() {
    return {
      form: {
        oldPassword: "",
        newPassword: "",
        newPasswordConfirm: "",
        oldPasswordType: "password",
        newPasswordType: "password",
        newPasswordConfirmType: "password",
      },
      working: false,
      show: true,
      token: "",
    };
  },
  validations: {
    form: {
      oldPassword: {
        required,
      },
      newPassword: {
        required,
      },
      newPasswordConfirm: {
        required,
      },
    },
  },
  methods: {
    toggleOldPasswordView() {
      this.form.oldPasswordType =
        this.form.oldPasswordType === "password" ? "text" : "password";
    },
    toggleNewPasswordView() {
      this.form.newPasswordType =
        this.form.newPasswordType === "password" ? "text" : "password";
    },
    passwordView(type) {
      return type === "password" ? "eye" : "eye-slash";
    },
    submit(evt) {
      const self = this;

      evt.preventDefault();

      if (this.$v.$invalid) {
        return;
      }

      if (this.form.newPassword !== this.form.newPasswordConfirm) {
        self.$store.dispatch("addAlert", {
          message: self.$i18n.t("password-mismatch"),
          name: "password-mismatch",
        });
        return;
      }

      this.$api.post("users/setpassword", {
        token: this.token,
        oldPassword: this.form.oldPassword,
        newPassword: this.form.newPassword,
      })
      .finally(function(){
        self.$store.dispatch("logout");
      });
    },
  },
  beforeMount() {
    if (this.$route.params.token) {
      this.token = this.$route.params.token;
    }
  },
};
</script>