<template>
  <div>
      <pre>
          {{data}}
      </pre>
  </div>
</template>

<script>
export default {
    computed: {
        data() {
            return JSON.stringify(this.$store.getters.data, null, 2)
        },
    },
}
</script>

<style>

</style>