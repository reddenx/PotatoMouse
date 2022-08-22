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
      class="mousepad"
      @contextmenu="eatContext"
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


<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

.mousepad {
  border: 1px solid black;
  height: 1200px;
  width: 1200px;
  touch-action: none;
}
</style>

<script>
import { Connection } from "./scripts/socket";
import { Mousepad } from "./scripts/mousepad";

export default {
  name: "App",
  components: {},
  data() {
    return {
      connectText: "ws://192.168.0.59:37075",
      sendText: "test",
      receiveText: "",
      connection: null,
      pad: null,
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
    this.pad = new Mousepad(this.connection);
  },
  methods: {
    connectPressed() {
      this.connection.connect(this.connectText);
    },
    sendPressed() {
      // let cmd = {
      //   type: 'mouseClick',
      //   data: 'left'
      // }
      // this.connection.send(JSON.stringify(cmd));
      // this.connection.send(this.sendText);
    },
    handleMessage(message) {
      this.receiveText = message;
    },
    handleDisconnect() {},

    mpdown() {
      // console.log("mpdown");
    },
    mpup() {
      // console.log("mpup");
    },

    mpenter() {
      // console.log("mpenter");
    },
    mpover() {
      // console.log("mpover");
    },

    mpmove() {
      //console.log("mpmove");
    },

    mpleave() {
      // console.log("mpleave");
    },
    mpout() {
      // console.log("mpout");
    },

    mpwheel() {
      // console.log("mpwheel");
    },

    tstart(e) {
      // console.log("tstart", e);
      [...e.changedTouches].forEach((t) =>
        this.pad.inputTouchDown(t.screenX, t.screenY, t.identifier)
      );
    },
    tmove(e) {
      //console.log("tmove", e);
      [...e.changedTouches].forEach((t) =>
        this.pad.inputTouchPositionChanged(t.screenX, t.screenY, t.identifier)
      );
    },
    tcancel(e) {
      // console.log("tcancel", e);
      [...e.changedTouches].forEach((t) =>
        this.pad.inputTouchUp(t.screenX, t.screenY, t.identifier)
      );
    },
    tend(e) {
      // console.log("tend", e);
      [...e.changedTouches].forEach((t) =>
        this.pad.inputTouchUp(t.screenX, t.screenY, t.identifier)
      );
    },
    eatContext(e) {
      // console.log("contextmenue", e);
      e.preventDefault();
      e.stopPropagation();
      return false;
    }
  },
};
</script>

