<template>
  <div>
    TEST:
    <button @click="buttonPressed">CLICK ME</button>
    <div style="padding: 1em; border: 1px solid red">{{ output }}</div>
    <div v-for="(input, id) in inputs" :key="id">{{ input }}</div>
    <video
      style="border: 1px solid blue; width: 800px; height: 800px"
      ref="videoTest"
    ></video>
  </div>
</template>

<script>
import { BrowserQRCodeReader } from "@zxing/library";

export default {
  components: {},
  data: () => ({
    output: "",
    showCamera: false,
    reader: {},
    inputs: [],
    stream: null,
  }),
  async mounted() {},
  methods: {
    async buttonPressed() {
      this.showCamera = !this.showCamera;

      // let stream = await navigator.mediaDevices.getUserMedia({
      //   video: { facingMode: "environment" },
      //   audio: false,
      // });

      // console.log(stream);

      let reader = new BrowserQRCodeReader(1000);
      let devices = await reader.listVideoInputDevices();

      console.log(devices);
      // devices.filter(d => d.label.)
      this.inputs = devices.map((d) => JSON.stringify(d));

      reader
        .decodeOnceFromVideoDevice(devices[0].deviceId, this.$refs.videoTest)
        .then((result) => {
          this.inputs.push(result);
          reader.reset();
        })
        .catch((err) => {
          this.inputs.push(err);
          reader.reset();
        });

      this.$refs.videoTest.play();
    },
    // startScanning() {},
    // stopScanning() {
    //   this.stream &&
    //     this.stream.getTracks().forEach(function (track) {
    //       track.stop();
    //     });
    //   this.stream = null;
    // },
  },
};

function decodeOnce(codeReader, selectedDeviceId) {}
</script>

<style>
</style>