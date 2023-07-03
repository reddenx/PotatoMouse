<template>
    <div class="container">
        <div v-if="connectionStatus == 'disconnected'">
            <button
                type="button"
                class="reconnect-button"
                @click="connectButtonPressed"
                :disabled="isAttemptingConnection"
            >
                Reconnect
            </button>
        </div>

        <div v-if="connectionStatus == 'connected'">
            <div class="control-container">
                <div class="control-row">
                    <span v-if="isDebugMode">{{
                        mousepad.currentState.name
                    }}</span>
                    <input
                        type="text"
                        class="send-input"
                        v-model="keyboardInputText"
                    />
                    <button @click="sendKeyboardText" class="send-button">
                        Send
                    </button>
                </div>
                <div class="control-row">
                    <button class="media-button" @click="handlePrevPressed">
                        &#x23EA;&#xFE0E;
                    </button>
                    <button class="media-button" @click="handlePausePressed">
                        &#x23f8;/&#x23f5;
                    </button>
                    <button class="media-button" @click="handleNextPressed">
                        &#x23E9;&#xFE0E;
                    </button>
                </div>
            </div>
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
import { Mousepad, CMD } from "./scripts/mousepad";

const CONNECTION_PORT = "37075";
const CONNECTION_PREFIX = "ws://";

export default {
    data: () => ({
        connectionStatus: "disconnected",
        isDebugMode: false,
        keyboardInputText: "",
        connection: new Connection(),
        connectionString: "",
        mousepad: {},
        isAttemptingConnection: false,
    }),
    computed: {
        formattedConnectionString() {
            return `${CONNECTION_PREFIX}${this.connectionString}:${CONNECTION_PORT}`;
        },
    },
    mounted() {
        //register connection handling
        this.connection.registerMessageHandler(this.handleMessage);
        this.connection.registerDisconnectHandler(this.handleDisconnect);
        this.connection.registerConnectHandler(this.handleConnect);

        //setup mousepad with connection
        this.mousepad = new Mousepad(this.connection);

        //parse data from uri, uses host to connect or override ip in first route after /#/
        let parts = window.location.href.split("/");
        let hostname = window.location.hostname;
        this.connectionString = hostname;
        if (parts.includes("#")) {
            let overrideIp = parts[parts.lastIndexOf("#") + 1];
            this.connectionString = overrideIp || hostname;
        }
        this.isDebugMode = parts.includes("debug");

        this.connectButtonPressed();
    },
    methods: {
        connectButtonPressed() {
            this.isAttemptingConnection = true;
            this.connection.connect(this.formattedConnectionString);
        },
        async sendKeyboardText() {
            if (!this.keyboardInputText) return;

            await this.connection.send(
                CMD("keyboardString", this.keyboardInputText)
            );
            this.keyboardInputText = "";
        },

        transitionToConnected() {
            this.connectionStatus = "connected";
        },
        transitionToDisconnected() {
            this.connectionStatus = "disconnected";
        },
        handleDisconnect() {
            if (this.connectionStatus == "disconnected") {
                this.isAttemptingConnection = false;
            } else this.transitionToDisconnected();
        },
        handleMessage(msg) {},
        handleConnect() {
            this.isAttemptingConnection = false;
            this.transitionToConnected();
        },

        async handlePrevPressed() {
            await this.connection.send(
                CMD("keyboardString", "{MediaPreviousTrack}")
            );
        },
        async handlePausePressed() {
            await this.connection.send(
                CMD("keyboardString", "{MediaPlayPause}")
            );
        },
        async handleNextPressed() {
            await this.connection.send(
                CMD("keyboardString", "{MediaNextTrack}")
            );
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
.mousepad {
    height: 100vh;
    width: 100%;
    overflow: hidden;
    background-color: gray;
    touch-action: none;
}

.media-button {
    height: 3em;
    width: 3em;
    font-size: 1em;
    letter-spacing: -2px;
    white-space: nowrap;
    text-align: center;;
}

.control-row {
    width: 100%;
    margin: auto;
    text-align: center;
}

.reconnect-button {
    width: 100vw;
    height: 100vh;
}

.container {
    margin: 0;
    padding: 0;
    overflow: hidden;
    max-height: 100vh;
}

.control-container {
    position: fixed;
    width: 100%;
}

.send-input {
    width: calc(100% - 6em);
    box-sizing: border-box;
    font-size: 16px;
}
.send-button {
    width: 6em;
    font-size: 16px;
}
</style>