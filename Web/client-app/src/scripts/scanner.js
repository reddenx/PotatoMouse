const video = document.createElement("video");
const canvasElement = document.getElementById("qr-canvas");
const canvas = canvasElement.getContext("2d");

const qrResult = document.getElementById("qr-result");
const outputData = document.getElementById("outputData");
const btnScanQR = document.getElementById("btn-scan-qr");


# YOU LEFT OFF HERE
//   https://codesandbox.io/s/ilrm9?file=/src/qrCodeScanner.js:1370-1376
//   https://ilrm9.csb.app/

export class Scanner {
    constructor(canvas, video) {
        this.qr = window.qrcode;
        this.running = false;
        this.busy = false;

        this.video = video;
        this.canvas = canvas;
        this.canvasContext = canvas.getContext('2s');
    }

    /**
     * 
     * @param {Function} qrCodeHandler function(string)
     */
    start(qrCodeHandler) {
        this.busy = true;
        navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } })
            .then(stream => {
                this.running = true;

                this.video.srcObject = stream;
                this.video.play();

                this.busy = false;
                updateLoop(this.canvas);
                scanLoop();
            });
    }
}

function updateLoop(canvas) {
    requestAnimationFrame(() => updateLoop(canvas));
}
function scanLoop() {
    window.qrcode.decode();
}