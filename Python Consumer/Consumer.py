from signalwire.relay.consumer import Consumer
import asyncio
devices = [
    [
  { 'to_number': '+1XXXXXXXXXX', 'from_number': '+1XXXXXXXXX' },
]
]
class CustomConsumer(Consumer):
  def setup(self):
    self.project = 'Project Here'
    self.token = 'Token Here'
    self.contexts = ['test']

  async def ready(self):
    print('Your consumer is ready!')

  async def on_incoming_call(self, call):
    result = await call.answer()
    if result.successful:
        action_1 = await call.play_audio_async('https://cdn.signalwire.com/default-music/welcome.mp3')
        await asyncio.sleep(5)
        await action_1.stop()
        action_2 = await call.connect(device_list=devices)

        print('Call answered..')
        print(call.from_number)


# Run your consumer..
consumer = CustomConsumer()
consumer.run()
