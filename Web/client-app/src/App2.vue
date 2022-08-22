<template>
  <div>
    TEST:
    <button @click="buttonPressed">CLICK ME</button>
    <div style="padding: 1em; border: 1px solid black;">{{ output }}</div>
    <canvas style="border: 1px solid black;" ref="test" v-show="showCamera"/>
  </div>
</template>

<script>
export default {
  data: () => ({
    output: "",
    showCamera: false,
  }),
  methods: {
    buttonPressed() {
        this.showCamera = true;
        console.log(this.$refs.test);

    },
  },
};
</script>

<style>
</style>