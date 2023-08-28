# DotNetTCP

* io.pipeline 버퍼 관리.
* tcp 소켓 통신.
* 패킷 핸들러로 함수 바인딩
* 패킷 큐로 모아 보냄.
* command console / file 로그
* 간단한 delta time 계산을 할 수 있습니다.

## Requirements

[.Net7](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## History

- pipereader 버퍼 4096 byte -> 65535 byte로 변경 함.

## To do

- [ ] 처리 시간이 긴 작업 혹은 루프는 Long-Running 옵션으로 태스크 새로 생성
- [ ] 락 변경
- [ ] I/O는 비동기, CPU Bound는 Task Run으로 별도처리
