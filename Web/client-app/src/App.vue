<template>
  <div id="app">
    connection: {{ isConnected }}
    <br />
    <input type="text" v-model="connectText" />
    <button type="button" @click="connectPressed">CONNECT</button>
    <button type="button" @click="sendPressed">SEND</button>
    <input type="text" v-model="sendText" />
    <br />
    <div>
      {{ receiveText }}
    </div>
    <div
      style="border: 1px solid black; height: 100px; width: 100px"
      @mousedown="mpdown"
      @mouseenter="mpenter"
      @mouseleave="mpleave"
      @mousemove="mpmove"
      @mouseout="mpout"
      @mouseover="mpover"
      @mouseup="mpup"
      @mousewheel="mpwheel"
      @touchstart="tstart"
      @touchmove="tmove"
      @touchcancel="tcancel"
      @touchend="tend"
    ></div>
  </div>
</template>

<script>
import { Connection } from "./scripts/socket";

export default {
  name: "App",
  components: {},
  data() {
    return {
      connectText: "ws://127.0.0.1:37070",
      sendText: "test",
      receiveText: "",
      connection: null,
    };
  },
  computed: {
    isConnected() {
      return (
        this.connection &&
        (this.connection.busy
          ? "busy"
          : this.connection.connected
          ? "connected"
          : "disconnected")
      );
    },
  },
  mounted() {
    this.connection = new Connection();
    this.connection.registerMessageHandler(this.handleMessage);
  },
  methods: {
    connectPressed() {
      this.connection.connect(this.connectText);
    },
    sendPressed() {
      this.connection.send(this.sendText);
    },
    handleMessage(message) {
      this.receiveText = message;
    },
    handleDisconnect() {},

    mpdown() {
      console.log("mpdown");
    },
    mpup() {
      console.log("mpup");
    },

    mpenter() {
      console.log("mpenter");
    },
    mpover() {
      console.log("mpover");
    },

    mpmove() {
      //console.log("mpmove");
    },

    mpleave() {
      console.log("mpleave");
    },
    mpout() {
      console.log("mpout");
    },

    mpwheel() {
      console.log("mpwheel");
    },

    tstart() {
      console.log("tstart");
    },
    tmove() {
      console.log("tmove");
    },
    tcancel() {
      console.log("tcancel");
    },
    tend() {
      console.log("tend");
    },
  },
};
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
