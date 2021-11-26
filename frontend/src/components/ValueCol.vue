<template>
        <!-- v-bind:key="header"> -->
    <span
        v-bind:class="medalColor(item)">
        <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
                <span v-bind="attrs" v-on="on">{{ getValue(item, header) }}</span>
            </template>
            <span>{{item.name}}</span>
        </v-tooltip>
    </span>

</template>

<script>
import { getMedalColor } from './tablehelpers'

export default {
    props: ["item", "header", "getValue"],
    methods: {
        medalColor(item) {
            return getMedalColor(item, this.header)
        }
    }
}
</script>

<style>
</style>