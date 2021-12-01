<template>
    <span :key="header.value">
        <span v-if="!header.hint" v-html=header.text></span>
        <span v-if="header.hint">
            <v-tooltip top>
                <template v-slot:activator="{ on }">
                    <span v-on="on">{{header.text}}</span>
                </template>
                <span>{{header.hint}}</span>
            </v-tooltip>
        </span>
    </span>

</template>

<script>
export default {
    props: ["header"]
}
</script>

<style>

</style>