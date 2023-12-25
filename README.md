# otlp dev dashboard

本项目是将 [aspire.dashboard](https://github.com/dotnet/aspire) 进行抽离，可以单独使用，基于OTLP Protocol

使用方式参见test中示例。

- 第一步
  创建dashboard的托管host。目前建议是空的asp.net core项目即可
  
- 第二步
  再其他项目中，复制test项目中的Extension扩展。并在启动的时候进行 `builder.AddServiceDefaults();` 即可。

`dashboard` 使用的是OTLP默认GRPC端口。

由于asire核心组件DCP并没有开源，暂时只有trace、metrics、structuredlog可用。

