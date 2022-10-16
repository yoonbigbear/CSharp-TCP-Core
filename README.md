# DotNetTCP

.Net 6 / Tcp socket server

## Requirements

[.Net6](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## How to use

You can check out [examples](https://github.com/yoonbigbear/DotNetTCP/tree/main/example)

## Keep in mind

- The default pipeReader buffer size is 4096. But in this project, I changed to 65535.
  If received data is over the maximum value can cause unexpected errors.
