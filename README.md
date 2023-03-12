# DotNetTCP

.Net 6 / Tcp socket server

Messaging packet between client and server through .net6 Tcp socket network. Recevied packets can handled with provided packet handler.
Send or receive packets are usually collected in an array and sent at once. However, you can also send it immediately at random.
On the other hands, proeject contains simple timer and file/console log.

net6와 io.pipeline을 사용해 tcp 소켓 패킷 통신을 합니다. 수신된 패킷은 패킷 핸들러를 통해 함수 바인딩이 가능하며, 패킷 송신은 패킷 큐를 통해 모아서 보내기가 가능합니다.


그 외 command console, file I/O 로그를 지원하며, 간단한 delta time 계산을 할 수 있습니다.

## Requirements

[.Net6](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## Features
- .net6 tcp socket server/client
- IO.Pipeline for socket buffer
- Packet queue & bind handler

## How to use

You can check out [examples](https://github.com/yoonbigbear/DotNetTCP/tree/main/example)

## Keep in mind

- The default pipeReader buffer size is 4096. But in this project, I changed to 65535.
  If received data is over the maximum value can cause unexpected errors.
