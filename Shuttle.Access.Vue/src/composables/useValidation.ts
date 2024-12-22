import useVuelidate from "@vuelidate/core";
import { isRef } from "vue";

export function useValidation(rules: any, state: any) {
  const v$ = useVuelidate(rules, state);

  const message = (path: string): string | undefined => {
    const error = v$.value.$errors.find((item) => item.$propertyPath == path);

    if (!error) {
      return undefined;
    }

    return isRef(error.$message) ? error.$message.value : error.$message;
  };

  const errors = async () => {
    await v$.value.$validate();

    return v$.value.$errors;
  };

  return {
    v$: v$,
    message: message,
    errors: errors,
  };
}
