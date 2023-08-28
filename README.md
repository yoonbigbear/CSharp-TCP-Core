# DotNetTCP

* io.pipeline 버퍼 관리.
* tcp 소켓 통신.
* 패킷 핸들러로 함수 바인딩
* 패킷 큐로 모아 보냄.
* command console / file 로그
* 간단한 delta time 계산을 할 수 있습니다.

## Requirements

[.Net7](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## Example
```C#
public class Server : TCPCore.TCPServer
{
    // listening client connection internally
    public override void Start(short port)
    {
	base.Start(port);

	// Initialize some game server stuffs
    }

    // connected client callback
    public override void AfterAccept(Socket socketTask)
    {
       // do something with socketTask.. 
    }
}
```


## History

- pipereader 버퍼 4096 byte -> 65535 byte로 변경 함.

## To do

- [x] 처리 시간이 긴 작업 혹은 루프는 Long-Running 옵션으로 태스크 새로 생성
- [x] 락 변경
- [x] I/O는 비동기, CPU Bound는 Task Run으로 별도처리
