# DotNetTCP

* io.pipeline 버퍼 관리.
* tcp 소켓 통신.
* 패킷 핸들러로 함수 바인딩
* 패킷 큐로 모아 보냄.
* command console / file 로그
* 간단한 delta time 계산을 할 수 있습니다.

## Requirements

[.Net6](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## Info

- pipereader 버퍼크기는 4096이 기본. 65535로 변경 함.

## To do

- [ ] 처리 시간이 긴 작업은 쓰레드풀 사용하지 않고 별도 쓰레드를 사용하도록 변경
