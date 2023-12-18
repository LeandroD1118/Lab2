import base64
import json
import paho.mqtt.client as mqtt
import datetime

THE_BROKER = "eu1.cloud.thethings.network"
MY_TOPICS = 'v3/+/devices/+/up'
APP_ID = '1118'
API_KEY = "NNSXS.USO6N6UHSTWIYBG42IYHGS4MRO6BDRLUF7AQYHY.M5YZU2P6EUJD7CX4RQW2ZZUTR4DNZNEGJVXCM3JLDTMMMPZUXXKQ"

def format_time():
    tm = datetime.datetime.now()
    sf = tm.strftime('%Y-%m-%d %H:%M:%S.%f')
    return sf[:-4]

def on_connect(client, userdata, flags, rc):
    print(f'[+] Connected to: {client._host}, port: {client._port}')
    print(f'Flags: {flags},  return code: {rc}')
    client.subscribe(MY_TOPICS, qos=0)        # Subscribe to all topics

def on_subscribe(mosq, obj, mid, granted_qos):
    print(f'Subscribed to topics: {MY_TOPICS}')
    print('Waiting for messages...')

def on_message(client, userdata, msg):
    themsg = str(msg.payload)
    print(f'\nReceived topic: {str(msg.topic)} with payload: {themsg}, at subscribers local time: {format_time()}')

    try:
        payload_str = msg.payload.decode('utf-8')
        payload = json.loads(payload_str)
        print(f'\nValid JSON: {json.dumps(payload)}')

        # Additional processing for sensor data
        if 'end_device_ids' in payload:
            device_id = payload['end_device_ids'].get('device_id', '')
            application_id = payload['end_device_ids']['application_ids'].get('application_id', '')
            frm_payload = payload.get('uplink_message', {}).get('frm_payload', '')

            print(f"\nPretty sensor output from topic: {msg.topic}")
            print(f"device_id: {device_id}")
            print(f"application_id: {application_id}")
            print(f"frm_payload: {frm_payload}")

            if frm_payload:
                decoded_payload = base64.b64decode(frm_payload).decode('utf-8')
                print(f"frm_payload_b64decode: {decoded_payload}")

    except json.JSONDecodeError as json_err:
        print(f"Error decoding payload. Payload may not be in JSON format.")
        print(f"Raw payload: {msg.payload}")

def on_disconnect():
    print("disconnect")
    client.disconnect()

client = mqtt.Client()

client.on_connect = on_connect
client.on_message = on_message
client.on_subscribe = on_subscribe
client.on_disconnect = on_disconnect

print(f'Connecting to TTN V3: {THE_BROKER}')
# Setup authentication from settings above
client.username_pw_set(APP_ID, API_KEY)

try:
    # IMPORTANT - this enables the encryption of messages
    client.tls_set()	# default certification authority of the local system
    client.connect(THE_BROKER, port=8883, keepalive=60)

except BaseException as ex:
    print(f'Cannot connect to TTN V3: {THE_BROKER}')
    print(f"TTN V3 error: {ex}")

client.loop_forever()
