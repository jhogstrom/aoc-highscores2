<template>
<div>
<span v-for="t, index in times" :key="index"><star :times="t"></star></span>
</div>
</template>

<script>
import Star from './Star.vue'

export default {
    components: { Star },
    props: ['times'],
}
</script>

<style>

</style>