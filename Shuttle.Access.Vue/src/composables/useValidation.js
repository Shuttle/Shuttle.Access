import useVuelidate from '@vuelidate/core';

export function useValidation(rules, state) {
    const v$ = useVuelidate(rules, state);
    
    const message = (path) => {
        return v$.value.$errors.find(item => item.$propertyPath == path)?.$message;
    }

    const errors = async () => {
        await v$.value.$validate();

        return v$.value.$errors;
    }

    return {
        v$: v$,
        message: message,
        errors: errors
    }
};