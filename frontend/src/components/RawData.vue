<template>
  <div>
      <pre>
          {{data}}
      </pre>
  </div>
</template>

<script>
const stringify = require("json-stringify-pretty-compact");
export default {
    computed: {
        data() {
            return stringify(this.$store.getters.data, {maxLength: 150})
        },
    },
}
</script>

<style>

</style>