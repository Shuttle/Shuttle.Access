import type { Breadcrumb, BreadcrumbStoreState } from "@/access";
import { defineStore } from "pinia";

export const useBreadcrumbStore = defineStore("breadcrumb", {
  state: (): BreadcrumbStoreState => {
    return {
      breadcrumbs: [],
    };
  },
  actions: {
    clear() {
      this.breadcrumbs = [];
    },
    addBreadcrumb(breadcrumb: Breadcrumb) {
      this.breadcrumbs.push(breadcrumb);
    },
    removeBreadcrumbsAfter(index: number) {
      this.breadcrumbs = this.breadcrumbs.slice(0, index + 1);
    },
  },
});
