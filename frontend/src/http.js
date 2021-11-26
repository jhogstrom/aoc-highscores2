const APIID = "lqujha07ue"
const STAGE = 'prod'
const REGION = "us-east-2"
export const ROOTURL = `https://${APIID}.execute-api.${REGION}.amazonaws.com/${STAGE}`
// import store from '@/store'

// const isDev = false

// import process from 'process'

// const ROOTURL = "http://localhost:8081/prod" // process.env.VUE_APP_URL

export class HttpApi {
    static async dofetch(resource, info) {
        const fullUrl = `${ROOTURL}${resource}`
        console.log("URL", fullUrl)
        try {
            const response = await fetch(fullUrl, info)
                .then(body => {
                    this.clearMessage()
                    return body
                })
            const result = await response.json()
            return result
        } catch (err) {
            alert(err);
        }
    }
    static log(msg) {
        // store.dispatch('setMessage', msg)
        console.log(msg)
    }
    static clearMessage() {
        // store.dispatch('clear')
    }
    static async get(resource) {
        const action = "GET from " + resource
        this.log(action)
        return await this.dofetch(resource)
    }

    static async post(resource, body) {
        const action = "POST TO " + resource
        this.log(action)
        return await this.dofetch(resource,
        {
            method: 'POST',
            // mode: 'no-cors', // no-cors
            body: JSON.stringify(body),
            headers: {"Content-Type": "application/json" }
        })
    }

    static async put(resource, body) {
        const action = "PUT TO " + resource
        this.log(action)
        return await this.dofetch(resource,
        {
            method: 'PUT',
            body: JSON.stringify(body),
            headers: {"Content-Type": "application/json" }
        })
    }
}
