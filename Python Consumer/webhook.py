import base64
import os
import requests
from signalwire.rest import Client as signalwire_client
from flask import Flask, request
from pyngrok import ngrok
from dotenv import load_dotenv
from signalwire.voice_response import VoiceResponse

load_dotenv()
ProjectID = os.getenv('PROJECTID')
Token = os.getenv('AUTHTOKEN')
Space = os.getenv('SPACENAME')
Relay_Num = os.getenv('FROM_NUM')
client = signalwire_client(ProjectID, Token, signalwire_space_url=Space)

app = Flask(__name__)


def start_ngrok():
    tunnel_url = ngrok.connect(5000).public_url
    auth = encoding(f"{ProjectID}:{Token}")
    headers = {
        'Accept': 'application/json',
        'Authorization': f'Basic {auth}'
    }

    sid_url = f"https://{Space}/api/laml/2010-04-01/Accounts/{ProjectID}/IncomingPhoneNumbers?PhoneNumber={os.getenv('WEBHOOK_NUM')}"
    sid_response = requests.request("GET", sid_url, headers=headers, data={}).json()
    SID = sid_response['incoming_phone_numbers'][0]['sid']
    update_url = f"https://{Space}/api/relay/rest/phone_numbers/{SID}"
    payload = {
        "call_handler": "laml_webhooks",
        "call_receive_mode": "voice",
        "call_request_url": f"{tunnel_url}/Recording"
    }
    update_response = requests.request("PUT", update_url, headers=headers, data=payload)
    print(update_response)
    print(f"Your URL is {tunnel_url}")


def encoding(key):
    key = key.encode("UTF-8")
    key_bytes = base64.b64encode(key)
    key_encoded = key_bytes.decode('UTF-8')
    return key_encoded


@app.route('/Recording', methods=['GET', 'POST'])
def record():
    response = VoiceResponse()
    response.record(transcribe=True, transcribe_callback='/Transcription', trim='trim-silence', timeout=10)
    return str(response)


@app.route('/Transcription', methods=['POST'])
def transcribe():
    auth = encoding(f"{ProjectID}:{Token}")
    text = request.values.get('TranscriptionText')
    cus_num = request.values.get('From').replace("+", "%2B")
    from_num = os.getenv('RELAY_NUM').replace("+", "%2B")
    record_uri = request.values.get('RecordingUrl')
    recording_file = requests.request("GET", f"https://{Space}{record_uri}").url
    print(recording_file)
    CallSid = request.values.get('CallSid')
    url = f"https://{Space}/api/laml/2010-04-01/Accounts/{ProjectID}/Messages"
    body = f"Thank you for testing this application. Transcription for call {CallSid}\nTranscription: {text}\nYou can listen to the calls recording here: {recording_file}"
    payload = f"Body={body}&To={cus_num}&From={from_num}"
    headers = {
        'Content-Type': 'application/x-www-form-urlencoded',
        'Accept': 'application/json',
        'Authorization': f'Basic {auth}'
    }
    response = requests.request("POST", url, headers=headers, data=payload)
    print(response)
    print(text)
    return "200"


if __name__ == '__main__':
    if os.environ.get('WERKZEUG_RUN_MAIN') != 'true':
        print("start ngrok tunnel")
        start_ngrok()
    app.run(debug=True)
