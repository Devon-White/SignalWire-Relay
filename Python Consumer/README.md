# Python IVR

## Remember to install the following packages:

### Prerequisites

#### python_conumer

```
import base64
from asyncio import sleep
import requests
import os
from dotenv import load_dotenv
from signalwire.relay.consumer import Consumer
import asyncio
from subprocess import Popen
```

#### Webhook

```
import base64
import os
import requests
from signalwire.rest import Client as signalwire_client
from flask import Flask, request
from pyngrok import ngrok
from dotenv import load_dotenv
from signalwire.voice_response import VoiceResponse
```

#### env

##### Remember to fill out the following information:

```js
PROJECTID=PROJECTID-HERE
AUTHTOKEN=AUTH-TOKEN-HERE
SPACENAME=SPACENAMEHERE.signalwire.com
WEBHOOK_NUM=SW-Number-Using-NGROK-Webhook (Needs SMS and voice capabilities)
RELAY_NUM=SW-Number-Hosting-Relay (Needs SMS and voice capabilities)
PERSONAL_NUM=Personal-Number (Needs SMS and voice capabilities)
CONTEXT=Relay-Context-Here
```

Convert `env` into a hidden file by adding a period in front on the filename `.env`

Ensure your SignalWire Numbers have messaging and Voice capabilities


### Instructions on how to use:

Run `python_consumer` to start a active consumer and ngrok tunnel. When the `consumer` is ready, it will send a sms to your `Perosnal Phone`. Respond with `Y` for the Relay system to place a outbound call to your personal. Once connected, your leg of the call will then be forwarded to the Relay system which will be handled as a `on_incoming_call`. From here, follow instructions given via `Text-to-Speech`
