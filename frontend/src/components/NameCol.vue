<template>
<div>
    <span>{{item.name}}</span>
    <span v-if="item.supporter"> (AoC++)</span>
</div>
</template>

<script>
export default {
    props: ["item"],
}
</script>

<style>

</style>