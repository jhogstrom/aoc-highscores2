<template>
    <v-menu
        bottom
        origin="center center"
        transition="scale-transition"
        offset-y
        >
        <template v-slot:activator="{ on, attrs }">
            <v-btn
                color="primary"
                dark
                v-bind="attrs"
                v-on="on"
                >
                {{ caption }}
            </v-btn>
        </template>

        <v-list>
            <v-list-item v-for="(item, i) in menuItems" :key="i">
                <v-list-item-title>
                    <span id="menuitem" @click="menuSelect(item)">
                    {{ item.title }}
                    </span>
                </v-list-item-title>
            </v-list-item>
        </v-list>
    </v-menu>
</template>

<script>
export default {
    props: ["caption", "menuItems"],
    methods: {
        menuSelect(item) {
            this.$emit("menuSelected", item)
        }
    }
}
</script>

<style>
div.v-list-item__title:hover {
    background-color: black;
    color: blanchedalmond;
    cursor: pointer;
}
</style>