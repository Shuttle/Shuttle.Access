<template>
    <form @submit.prevent="submit" class="sv-form">
        <a-title :title="$t('role-upload')" close-drawer type="borderless" />
        <v-file-upload density="comfortable" v-model="state.file" :icon="`svg:${mdiCloudUploadOutline}`"
            :title="$t('drag-and-drop-file')" :multiple="false"></v-file-upload>
        <div>{{ state.file?.value }}</div>
        <v-alert class="mt-2" variant="outlined" type="error" v-if="validation.message('file')">
            {{ validation.message('file') }}
        </v-alert>
        <div class="sv-strip sv-strip--reverse mt-2">
            <v-btn type="submit" :disabled="busy">{{ $t("submit") }}</v-btn>
        </div>
    </form>
</template>

<script setup lang="ts">
import { computed, reactive, type Reactive, type ShallowRef } from "vue";
import { helpers } from '@vuelidate/validators';
import { useValidation } from "@/composables/Validation"
import api from "@/api";
import { useDrawerStore } from "@/stores/drawer";
import { useSnackbarStore } from "@/stores/snackbar";
import { useI18n } from "vue-i18n";
import { mdiCloudUploadOutline } from "@mdi/js";

const drawerStore = useDrawerStore()
const { t } = useI18n({ useScope: 'global' });

const busy: Ref<boolean> = ref(false);

type State = {
    file: ShallowRef | null
}

const state: Reactive<State> = reactive({
    file: shallowRef(null),
});

const rules = computed(() => {
    return {
        file: { required: helpers.withMessage(t('messages.file-required'), () => state.file || state.file?.value.name) }
    }
});

const validation = useValidation(rules, state);

const submit = async () => {
    const errors = await validation.errors();

    if (errors.length) {
        return;
    }

    busy.value = true;

    try {
        const formData = new FormData();

        formData.append("file", state.file);

        await api.post('v1/roles/file', formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        })

        useSnackbarStore().requestSent();

        drawerStore.close();
    } finally {
        busy.value = false;
    }
}
</script>
