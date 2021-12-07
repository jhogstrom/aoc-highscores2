<template>
      <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
                <span v-bind="attrs" v-on="on">{{item.name}}</span>
                <span v-if="item.supporter"> (AoC++)</span>
            </template>
            <span>AoC id {{item.id}}</span>
        </v-tooltip>
</template>

<script>
export default {
    props: ["item"],
}
</script>

<style>

</style>