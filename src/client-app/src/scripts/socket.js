export class Connection {
    constructor() {
        /** @type {Boolean} */
        this.connected = false;
        /** @type {Boolean} */
        this.busy = false;
        /** @type {WebSocket} */
        this.socket = null;
        /** @type {Array<(message: String) => any>} */
        this.messageHandlers = [];
        /** @type {Array<() => any>} */
        this.disconnectHandlers = [];
        /** @type {Array<() => any>} */
        this.connectHandlers = [];
    }

    /** @param {String} host */
    async connect(host) {
        if (this.busy)
            return;
        if (this.connected)
            await this.disconnect();

        //might as well double check lock /shrug
        if (this.connected)
            return;

        this.connected = true;
        this.busy = true;

        try {
            this.socket = new WebSocket(host);
        }
        catch {
            this.busy = false;
            this.connected = false;
            return;
        }
        this.socket.addEventListener('open', event => {
            console.log('open', event);
            this.busy = false;
            this.connectHandlers.forEach(d => d());
        });
        this.socket.addEventListener('message', event => {
            console.log('message', event);

            this.messageHandlers.forEach(m => m(event.data));
        });
        this.socket.addEventListener('close', event => {
            console.log('close', event);
            this.busy = false;

            this.connected = false;
            this.socket = null;
            this.disconnectHandlers.forEach(d => d());
        });
        this.socket.addEventListener('error', event => {
            console.log('error', event);
        });
    }
    async disconnect() {
        if (this.busy)
            return;

        if (this.connected) {
            this.socket.close();
            this.busy = true;
            this.connected = false;
            this.socket = null;
        }
    }
    async send(message) {
        if (this.busy)
            return;
        if (this.connected) {
            this.socket.send(message);
        }
    }
    /**
     * 
     * @param {(message: String) => any} handler 
     */
    registerMessageHandler(handler) {
        this.messageHandlers.push(handler);
    }
    /**
     * 
     * @param {() => any} handler 
     */
    registerDisconnectHandler(handler) {
        this.disconnectHandlers.push(handler);
    }
    /**
     * 
     * @param {() => any} handler 
     */
    registerConnectHandler(handler) {
        this.connectHandlers.push(handler);
    }
}