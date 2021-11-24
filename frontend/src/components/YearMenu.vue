<template>
    <v-menu
        bottom
        origin="center center"
        transition="scale-transition"
        offset-y
        >
        <template v-slot:activator="{ on, attrs }">
            <v-btn
                class="menubutton"
                color="cyan darken-2"
                dark
                v-bind="attrs"
                v-on="on"
                >
                Years
            </v-btn>
        </template>

        <v-list>
            <v-list-item v-for="(item, i) in menuItems" :key="i">
                <v-list-item-title >
                    <a :href="item.url">{{item.text}}</a>
                </v-list-item-title>
            </v-list-item>
        </v-list>
    </v-menu>
</template>

<script>
export default {
    computed: {
        menuItems() {
            let res = []
            for (let i = 2016; i <= new Date().getFullYear(); i++) {
                res.push({ text: i, url: `/?year=${i}`})
            }
            console.log(res)
            return res
        }
    },
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
.menubutton {
  margin: 5px;
}

</style>