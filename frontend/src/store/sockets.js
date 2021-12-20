const websocketUrl = "wss://oxf6j0bjp8.execute-api.us-east-2.amazonaws.com/dev"
const emitter = require('events').EventEmitter;

class Sockets {
    constructor() {
        this.counter = 0
        this.connecting = false
        this.sessionOpen = false
        this.connect()
        this.messageQ = []
        this.keepAlive()
        this.callbacks = []
        this.emitter = new emitter();
        console.log("SOCKETS ALIVE")
    }
    connect() {
        this.connecting = true
        this.counter = 0
        console.log("Opening WS connection")
        this.connection = new WebSocket(websocketUrl)
        this.connection.onopen = (event) => this.onopen(event)
        this.connection.onclose = (event) => this.onclose(event)
        this.connection.onmessage = (event) => this.onmessage(event)
        this.connection.onerror = (event) => this.onerror(event)
    }
    logEvent(eventType, event) {
        try {
            console.log(eventType, JSON.parse(event))

        } catch (SyntaxError) {
            console.log(eventType, event)
        }
    }
    onopen(event) {
        this.sessionOpen = true
        this.connecting = false
        this.logEvent("WS/OPEN", event)
        for (const m of this.messageQ) {
            this.sendMessage(m)
        }
        this.messageQ = []
    }
    onmessage(event) {
        const payload = JSON.parse(event.data)
        this.emitter.emit("Payload", payload)
    }
    onclose(event) {
        this.logEvent("WS/CLOSE", event)
        this.sessionOpen = false
    }
    onerror(event) {
        this.logEvent("WS/ERROR", event)
    }
    sendMessage(message) {
        if (this.sessionOpen) {
            if (message) {
                console.log("Sending:", JSON.stringify(message))
            }
            this.connection.send(JSON.stringify(message));
        } else {
            console.log("WS/Send: connection is closed")
            this.messageQ.push(message)
            if (!this.connecting){
                this.connect()
            }
        }
    }
    ensureConnectionAndRegistration(payload) {
        if (!this.sessionOpen) {
            this.sendMessage(payload)
        }
    }
    keepAlive() {
        const timeout = 20000
        // Keep tunnel alive for 3600 seconds (by refreshing 3600/20 times)
        if (this.sessionOpen && this.counter < 3600/20) {
            this.sendMessage('')
            this.counter++
            setTimeout(this.keepAlive, timeout)
        }
    }
}

const sockets = new Sockets()
export {sockets}