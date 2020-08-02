<template>
    <div class="sv-title">
        <h1>
            {{ $t(text) }}
        </h1>
    </div>
</template>

<script>
export default {
    props: ['text']
}
</script>