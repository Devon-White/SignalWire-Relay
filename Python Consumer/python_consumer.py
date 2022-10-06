import base64
from asyncio import sleep
import requests
import os
from dotenv import load_dotenv
from signalwire.relay.consumer import Consumer
import asyncio
from subprocess import Popen

load_dotenv()


async def Menu(call):
    devices = [
        [
            {'to_number': os.getenv('WEBHOOK_NUM'), 'from_number': call.from_number},
        ]
    ]
    prompt_result = await call.prompt_tts(prompt_type='speech',
                                          text="Please say. Alpha. If you wish to connect to a new call, or, please say. Beta. If you wish to hear a affirmation.")
    if call.state == "ended":
        await call.hangup()
    else:
        print(call.from_number)
        if prompt_result.successful:
            response = prompt_result.result.lower()
            print(response)
            if response == "alpha":
                connect_result = await call.connect(device_list=devices)
                if connect_result.successful:
                    await Connected_Call(call)
                else:
                    await call.hangup()
            if response == "beta":
                await Affirmation(call)
            else:
                await call.play_tts(text="We did not recognize the response")
                await Menu(call)
        else:
            await call.play_tts(text="We did not recognize the response")
            await Menu(call)


async def Affirmation(call):
    req = requests.get(url="https://www.affirmations.dev").json()
    value = req['affirmation']
    print(value)
    play = await call.play_tts(value)
    if play.successful:
        await Menu(call)


async def Connected_Call(call):
    await call.play_tts("Please leave a message to be recorded")
    await sleep(5)
    await call.play_tts("Ending call and Transcribing recording.")
    await call.hangup()


def encoding(key):
    key = key.encode("UTF-8")
    key_bytes = base64.b64encode(key)
    key_encoded = key_bytes.decode('UTF-8')
    return key_encoded


class CustomConsumer(Consumer):
    def setup(self):
        self.project = os.getenv('PROJECTID')
        self.token = os.getenv('AUTHTOKEN')
        self.contexts = [os.getenv('CONTEXT')]

    async def ready(self):
        print('Your consumer is ready!')
        await self.initial_sms()
        Popen('python webhook.py')

    async def initial_sms(self):

        result = await self.client.messaging.send(context=os.getenv('CONTEXT'), from_number=os.getenv('RELAY_NUM'),
                                                  to_number=os.getenv('PERSONAL_NUM'), body="Respond with ( Y ) if "
                                                                                            "you wish to be connected"
                                                                                            " to a call.")
        if result.successful:
            print(result.message_id)

    async def on_incoming_call(self, call):
        result = await call.answer()
        if result.successful:
            await call.play_tts("Welcome to SignalWire!")
            play_audio = await call.play_audio_async('https://cdn.signalwire.com/default-music/welcome.mp3')
            await asyncio.sleep(5)
            await play_audio.stop()
            await Menu(call)

    async def on_incoming_message(self, message):
        devices = [
            [
                {'to_number': os.getenv('RELAY_NUM'), 'from_number': message.from_number},
            ]
        ]

        print(message)
        result = message.body.lower()

        if result == "y":
            send = await self.client.messaging.send(
                context=os.getenv('CONTEXT'),
                from_number=os.getenv('RELAY_NUM'),
                to_number=message.from_number,
                body=f"Calling you from {os.getenv('WEBHOOK_NUM')}")
            if send.successful:
                print(send.message_id)
                await sleep(2)
                call_dial = self.client.calling.new_call(from_number=os.getenv('webhook_num'),
                                                         to_number=message.from_number)
                call_result = await call_dial.dial()
                if await call_result.call.wait_for_answered():
                    detect_action = await call_result.call.detect_answering_machine()
                    if detect_action.successful:
                        if detect_action.result == "HUMAN":
                            print(detect_action.result)
                            await call_dial.play_tts("Connecting to Relay System")
                            connect = await call_result.call.connect(device_list=devices)
                            if connect.successful:
                                await sleep(2)
                                call_kill = await connect.call.wait_for_ended()
                                if call_kill:
                                    await call_result.call.hangup()
                                print('Killed Call...')
                            else:
                                print("Connect Failed...")
                                await call_result.call.hangup()
                        else:
                            print(detect_action.result)
                            await call_result.call.hangup()
        else:
            url = "https://blog.cdn.own3d.tv/resize=fit:crop,height:400,width:600/Di50KNmSOqaanbsVJkQB"
            send = await self.client.messaging.send(
                context=os.getenv('CONTEXT'),
                from_number=os.getenv('RELAY_NUM'),
                to_number=message.from_number,
                media=f"{url}")


consumer = CustomConsumer()
consumer.run()
