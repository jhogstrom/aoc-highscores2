<template>
<span>
    <span v-for="t, day in days" :key="day">
        <star :times="t" :day="day+1"></star>
    </span>
</span>
</template>

<script>
import Star from './Star.vue'

export default {
    components: { Star },
    props: ['days'],
}
</script>

<style>

</style>