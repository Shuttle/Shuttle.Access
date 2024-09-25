import { defineStore } from "pinia";

export interface OAuthProvider {
    name: string;
    url: string;
}

export interface OAuthStoreState {
    providers: OAuthProvider[]
}

export const useOAuthStore = defineStore("oauth", {
    state: (): OAuthStoreState => {
      return {
        providers: [{
            name: "github",
            url: "https://github.com/login/oauth/authorize?client_id=Iv23li348Vg4riZQSXft&redirect_uri=http://localhost:5599/v1/oauth"
        }],
      };
    }
});