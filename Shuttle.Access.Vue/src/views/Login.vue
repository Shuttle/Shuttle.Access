<template>
  <div class="container-fluid">
    <div class="row">
      <div class="col"></div>
      <div class="col-xl-3 col-lg-4 col-md-5 mx-auto p-0">
        <sa-title text="Login" />
        <div class="col">
          <b-form @submit="login" v-if="show" class="w-100">
            <b-form-group id="input-group-email" label="E-mail" label-for="input-email">
              <b-form-input
                id="input-email"
                v-model="form.email"
                type="email"
                required
                placeholder="E-mail address"
                class="mb-2"
              ></b-form-input>
            </b-form-group>

            <b-form-group id="input-group-password" label="Password" label-for="input-password">
              <b-form-input
                id="input-password"
                label="Password"
                v-model="form.password"
                required
                placeholder="Password"
                class="mb-2"
                type="password"
              ></b-form-input>
            </b-form-group>

            <div>
              <b-button class="float-right" variant="primary" type="submit" :disabled="working">
                <font-awesome-icon icon="circle-notch" class="fa-spin mr-2" v-if="working" />Login
              </b-button>
            </div>
          </b-form>
        </div>
      </div>
      <div class="col"></div>
    </div>
  </div>
</template>

<script>
import router from "../router";

export default {
  data() {
    return {
      form: {
        email: "",
        password: ""
      },
      show: true
    };
  },
  computed: {
    working() {
      return this.$store.state.working;
    }
  },
  methods: {
    login(evt) {
      evt.preventDefault();

      this.$store.dispatch("login", {
        username: this.form.email,
        password: this.form.password
      });
    },
    register() {
      router.replace("/register");
    }
  }
};
</script>