FROM python:3.7.9

WORKDIR /usr/GrpcWebUnity/

COPY ./Proto ./Proto

COPY ./Server/ ./Server/

WORKDIR /usr/GrpcWebUnity/Server/

RUN pip install --no-cache-dir -r requirements.txt

ENTRYPOINT ["python", "./src/main.py"]
