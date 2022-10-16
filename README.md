# DotNetTCPServerCore

.Net 6 / Tcp socket server

## Requirements

[.Net6](https://dotnet.microsoft.com/en-us/download) / [System.IO.Pipeline](https://www.nuget.org/packages/System.IO.Pipelines/)

## Recommend usage

- Implement a new server class that inherited TCPServer class and overrides virtual functions.

- PipeReader buffer has a limited byte size of 65535. If received data is over the maximum value can cause unexpected errors.
