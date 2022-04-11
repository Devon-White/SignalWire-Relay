const { RelayConsumer } = require('@signalwire/node')
const sleep = (seconds) => new Promise(resolve => setTimeout(resolve, seconds*1000))

async function delay(time) {

}

const consumer = new RelayConsumer({
  project: 'Project',
  token: 'Token',
  contexts: ['test'],
  onIncomingCall: async (call) => {
    await call.answer()
    console.log("Call Has Been Answered")

    await call.playTTS({text: 'Welcome to SignalWire!'})
    console.log("Saying: Welcome to SignalWire!")

    const playAction = await call.playAudioAsync('https://cdn.signalwire.com/default-music/welcome.mp3')
    console.log("Playing SW Intro for 5 seconds")
      await sleep(5)
      const stopResult = await playAction.stop()

      const connectResult = await call.connect(
          {type: 'phone', to: '+1XXXXXXXXXX', timeout: 20}
      )
      console.log("We connecting to the OGs")

      if (connectResult.successful) {
        console.log("We connected to the OGs")
        // connectResult.call is the remote leg connected with yours.
      }
    }
  })
consumer.run()
