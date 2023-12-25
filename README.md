# otlp dev dashboard

本项目是将 [aspire.dashboard](https://github.com/dotnet/aspire) 进行抽离，可以单
独使用，基于 OTLP Protocol

使用方式参见 test 中示例。

- 第一步创建 dashboard 的托管 host。目前建议是空的 asp.net core 项目即可
- 第二步再其他项目中，复制 test 项目中的 Extension 扩展。并在启动的时候进行
  `builder.AddServiceDefaults();` 即可。

`dashboard` 使用的是 OTLP 默认 GRPC 端口。

由于 asire 核心组件 DCP 并没有开源，暂时只有 trace、metrics、structuredlog 可用
。
