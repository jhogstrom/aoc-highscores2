const websocketUrl = "wss://oxf6j0bjp8.execute-api.us-east-2.amazonaws.com/dev"
const emitter = require('events').EventEmitter;

class Sockets {
    constructor() {
        this.connection = new WebSocket(websocketUrl)
        this.connection.onopen = (event) => this.onopen(event)
        this.connection.onmessage = (event) => this.onmessage(event)
        this.connection.onerror = (event) => this.onerror(event)
        this.counter = 0
        this.messageQ = []
        this.keepAlive()
        this.callbacks = []
        this.emitter = new emitter();
        console.log("SOCKETS ALIVE")
    }
    onopen(event) {
        console.log(event)
        console.log("Successfully connected to the websocket server...")
        for (const m of this.messageQ) {
            this.sendMessage(m)
        }
        this.messageQ = []
    }
    onmessage(event) {
        const payload = JSON.parse(event.data)
        this.emitter.emit("Payload", payload)
    }
    onerror(event) {
        console.log("ERROR", event);
    }
    sendMessage(message) {
        if (this.connection.readyState == this.connection.OPEN) {
            if (message) {
                console.log("Sending:", JSON.stringify(message))
            }
            this.connection.send(JSON.stringify(message));
        } else {
            console.log("connection is", this.connection.readyState)
            this.messageQ.push(message)
        }
    }
    keepAlive() {
        const timeout = 20000
        if (this.connection.readyState == this.connection.OPEN && this.counter < 7200/20) {
            this.sendMessage('')
            this.counter++
            setTimeout(this.keepAlive, timeout)
        }
    }
}

const sockets = new Sockets()
export {sockets}