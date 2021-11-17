<template>
    <v-expansion-panels>
      <v-expansion-panel>
        <v-expansion-panel-header>
            <div>
              <v-icon>mdi-information</v-icon>
            </div>
        </v-expansion-panel-header>
        <v-expansion-panel-content>
            <div v-html="infotext"></div>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-expansion-panels>
</template>

<script>
export default {
    props: ["infotext"]

}
</script>

<style>

</style>