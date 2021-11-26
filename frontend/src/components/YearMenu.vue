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
            <v-list-item v-for="item in menuItems" :key="item.url">
                <v-list-item-title >
                    <span v-if="isCurrent(item.year)">[{{ item.year}}]</span>
                    <a v-else :href="item.url">{{item.year}}</a>
                </v-list-item-title>
            </v-list-item>
        </v-list>
    </v-menu>
</template>

<script>
export default {
    computed: {
        currentYear() {
            return this.$store.getters.year
        },
        menuItems() {

            let res = []
            for (let year = 2016; year <= new Date().getFullYear(); year++) {
                res.push({ year: year, url: `/?year=${year}`})
            }
            return res
        }
    },
    methods: {
        isCurrent(year) {
            return String(year) == this.currentYear
        },
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