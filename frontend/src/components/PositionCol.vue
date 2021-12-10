<template>
    <v-tooltip bottom>
        <template v-slot:activator="{ on, attrs }">
            <span v-bind="attrs" v-on="on">{{item.position}}</span>
        </template>
        <span>{{item.name}}</span>
    </v-tooltip>

</template>

<script>
export default {
    props: ["item"]

}
</script>

<style>

</style>