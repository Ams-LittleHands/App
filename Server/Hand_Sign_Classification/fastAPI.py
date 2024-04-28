from fastapi import FastAPI, File, UploadFile, Request
from secrets import token_hex
from vtn_hc_inference import Inference
import requests 
import json
import azure.cognitiveservices.speech as speechsdk
from azure.cognitiveservices.speech import AudioDataStream
from io import BytesIO
import base64
from fastapi.responses import FileResponse
app = FastAPI()
infer = Inference()


@app.get('/')
def root():
    return {'message': 'Welcome to the Image Classification API'}


@app.post("/uploadVideo/")
async def create_upload_file(file: UploadFile):
    print("Received Video")
    # file_name = token_hex(file.filename)
    file_name = 'default'
    file_path = f"receivedVideo/{file_name}.mp4"
    # file_path = "./receivedVideo/8544a8fb511f0eea7fe2.mp4"
    with open(file_path,'wb') as f:
        content = await file.read()
        f.write(content)
    
    sentence,duration = infer.predict(file_path)
    return {'message': f'Results: {sentence}','duration': duration}



@app.post("/transformTextToSpeech/")
async def transform_text_to_speech(text: str):
    api_key = 'b86466fa4d914c128b2f24ced8267804'
    endpoint = 'https://eastus.api.cognitive.microsoft.com/',
    region = 'eastus'
    speech_config = speechsdk.SpeechConfig(subscription = api_key,region = region)
    audio_config = speechsdk.audio.AudioOutputConfig(use_default_speaker = True)
    speech_config.speech_synthesis_vioce_name = 'vi-VN-HoaiMyNeural'
    speech_synthesizer = speechsdk.SpeechSynthesizer(speech_config = speech_config,audio_config = audio_config)

    speech_synthesis_result = speech_synthesizer.speak_text_async(text)


    if speech_synthesis_result.get().reason == speechsdk.ResultReason.SynthesizingAudioCompleted:
        speech_synthesis_result.get()
        audio_data = speech_synthesis_result.get().audio_data
        with open('output.mp3','wb') as f:
            f.write(audio_data)
        
        headers = {'Content-Disposition': f'attachment; filename="output.mp3"'}
        return FileResponse("output.mp3", headers=headers, media_type="audio/mp3")
       

    else:
        return {'message': 'Failed to synthesize speech'}
