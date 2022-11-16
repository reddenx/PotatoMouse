<template>
  <div>
    <div v-if="mode == 'scanning'">
      <button
        class="start-scanning-button"
        @click="scanButtonPressed"
        :disabled="isAttemptingConnection"
      >
        Scan!
      </button>
      <input
        type="text"
        v-model="connectionString"
        :disabled="isAttemptingConnection"
      />
      <button
        type="button"
        @click="connectButtonPressed"
        :disabled="isCameraOn || isAttemptingConnection"
      >
        Connect!
      </button>
      <br />
      <div v-show="connectionString">
        {{ formattedConnectionString }}
      </div>
      <br />
      <video v-show="isCameraOn" class="camera-video" ref="video"></video>
    </div>
    <div v-if="mode == 'connected'">
      {{mousepad.currentState.name}}
      <div
        class="mousepad"
        @contextmenu="eatMousepadContextMenu"
        @touchstart="mousepadTouchStart"
        @touchmove="mousepadTouchMove"
        @touchcancel="mousepadTouchCancel"
        @touchend="mousepadTouchEnd"
      ></div>
    </div>
  </div>
</template>

<script>
import { Connection } from "./scripts/socket";
import { Mousepad } from "./scripts/mousepad";
import { BrowserQRCodeReader } from "@zxing/library";

const CONNECTION_PORT = "37075";
const CONNECTION_PREFIX = "ws://";

export default {
  data: () => ({
    mode: "scanning",
    isCameraOn: false,
    connection: new Connection(),
    connectionString: "",
    mousepad: {},
    reader: new BrowserQRCodeReader(1000),
    isAttemptingConnection: false,
  }),
  computed: {
    formattedConnectionString() {
      return `${CONNECTION_PREFIX}${this.connectionString}:${CONNECTION_PORT}`;
    },
  },
  mounted() {
    this.connection.registerMessageHandler(this.handleMessage);
    this.connection.registerDisconnectHandler(this.handleDisconnect);
    this.connection.registerConnectHandler(this.handleConnect);
    this.mousepad = new Mousepad(this.connection);

    let parts = window.location.href.split("/");
    let ip = parts[parts.lastIndexOf("#") + 1];
    if(ip) {
      this.connectionString = ip;
      this.connectButtonPressed();
    }
  },
  methods: {
    scanButtonPressed() {
      if (this.isCameraOn) this.stopScanning();
      else this.startScanning();
    },
    connectButtonPressed() {
      this.isAttemptingConnection = true;
      this.connection.connect(this.formattedConnectionString);
    },

    transitionToConnected() {
      this.reader.reset();
      this.isCameraOn = false;
      this.mode = "connected";
    },
    transitionToDisconnected() {
      this.mode = "scanning";
    },

    async startScanning() {
      this.isCameraOn = true;
      try {
        let devices = await this.reader.listVideoInputDevices();

        if (!devices.length) {
          this.isCameraOn = false;
          return;
        }

        let result = await this.reader.decodeOnceFromVideoDevice(
          devices[0].deviceId,
          this.$refs.video
        );
        this.reader.reset();
        this.isCameraOn = false;
        this.connectionString = result.getText();
        this.connectButtonPressed();
      } catch {
        this.reader.reset();
        this.isCameraOn = false;
      }
    },
    stopScanning() {
      this.reader.reset();
      this.isCameraOn = false;
    },
    handleDisconnect() {
      if (this.mode == "scanning") {
        this.isAttemptingConnection = false;
      } else this.transitionToDisconnected();
    },
    handleMessage(msg) {},
    handleConnect() {
      this.isAttemptingConnection = false;
      this.transitionToConnected();
    },

    eatMousepadContextMenu() {
      e.preventDefault();
      e.stopPropagation();
      return false;
    },
    mousepadTouchStart(e) {
      // console.log("tstart", e);
      [...e.changedTouches].forEach((t) =>
        this.mousepad.inputTouchDown(t.screenX, t.screenY, t.identifier)
      );
    },
    mousepadTouchMove(e) {
      //console.log("tmove", e);
      [...e.changedTouches].forEach((t) =>
        this.mousepad.inputTouchPositionChanged(
          t.screenX,
          t.screenY,
          t.identifier
        )
      );
    },
    mousepadTouchCancel(e) {
      // console.log("tcancel", e);
      [...e.changedTouches].forEach((t) =>
        this.mousepad.inputTouchUp(t.screenX, t.screenY, t.identifier)
      );
    },
    mousepadTouchEnd(e) {
      // console.log("tend", e);
      [...e.changedTouches].forEach((t) =>
        this.mousepad.inputTouchUp(t.screenX, t.screenY, t.identifier)
      );
    },
  },
};
</script>

<style>
.camera-video {
  min-width: 100vw;
  min-height: 100vh;
  border: 1px solid black;
}
.mousepad {
  height: 90vh;
  width: 90vw;
  background-color: gray;
  touch-action: none;
}
</style>