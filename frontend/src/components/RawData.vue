<template>
    <div>
        <info-block :infotext="infotext" :title="infoTitle"></info-block>
        <pre>
            {{data}}
        </pre>
    </div>
</template>

<script>
const stringify = require("json-stringify-pretty-compact");
import InfoBlock from './InfoBlock.vue'

export default {
    components: { InfoBlock },
    data() { return {
        infoTitle: "Raw data",
        infotext: "The downloaded data structure. Great for debugging :)"
    }},
    computed: {
        data() {
            return stringify(this.$store.getters.data, {maxLength: 150})
        },
    },
}
</script>

<style>

</style>