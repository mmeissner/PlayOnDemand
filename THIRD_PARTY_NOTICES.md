# Third-Party Notices

PlayOnDemand bundles or depends on the following third-party software. Each entry lists the upstream project, the license under which we receive it, and a pointer to the canonical source. The full license texts for libraries shipped with the kiosk live under `LeapVR.Shell.Build/License/`.

This file is best-effort. If something here is wrong or missing, please open an issue.

---

## Server (Pod.* NuGet dependencies — all under standard OSS licenses)

| Package | License | Source |
|---|---|---|
| `Microsoft.AspNetCore.App` (framework) | MIT | https://github.com/dotnet/aspnetcore |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | MIT | https://github.com/dotnet/aspnetcore |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | MIT | https://github.com/dotnet/aspnetcore |
| `Microsoft.EntityFrameworkCore` (+ `.InMemory`, `.Design`) | MIT | https://github.com/dotnet/efcore |
| `Microsoft.OpenApi` | MIT | https://github.com/microsoft/OpenAPI.NET |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL License | https://github.com/npgsql/efcore.pg |
| `Newtonsoft.Json` | MIT | https://github.com/JamesNK/Newtonsoft.Json |
| `Swashbuckle.AspNetCore` (+ `.Annotations`, `.Filters`) | MIT | https://github.com/domaindrivendev/Swashbuckle.AspNetCore |
| `Grpc.AspNetCore`, `Grpc.Net.Client`, `Grpc.Core`, `Grpc.Tools` | Apache 2.0 | https://github.com/grpc/grpc, https://github.com/grpc/grpc-dotnet |
| `Google.Protobuf` | BSD-3-Clause | https://github.com/protocolbuffers/protobuf |
| `AspNetCoreRateLimit` | MIT | https://github.com/stefanprodan/AspNetCoreRateLimit |
| `Certes` | MIT | https://github.com/fszlin/certes |
| `NLog.Web.AspNetCore`, `NLog.Extensions.Logging`, `NLog` | BSD-3-Clause | https://github.com/NLog/NLog |
| `MailKit`, `MimeKit` | MIT | https://github.com/jstedfast/MailKit, https://github.com/jstedfast/MimeKit |
| `xunit` (+ `xunit.runner.visualstudio`) | Apache 2.0 | https://github.com/xunit/xunit |
| `FluentAssertions` (v7.x — pre-license-change) | Apache 2.0 | https://github.com/fluentassertions/fluentassertions |
| `Moq` | BSD-3-Clause | https://github.com/devlooped/moq |
| `System.IdentityModel.Tokens.Jwt` | MIT | https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet |
| `Microsoft.AspNetCore.Mvc.Testing` | MIT | https://github.com/dotnet/aspnetcore |

## Kiosk (LeapVR.* — bundled libraries)

License texts are in `LeapVR.Shell.Build/License/`. Each filename matches the bundled library.

| Library | License | Source |
|---|---|---|
| `AutoMapper` | MIT | https://github.com/AutoMapper/AutoMapper |
| `Caliburn.Micro` | MIT | https://github.com/Caliburn-Micro/Caliburn.Micro |
| `DotNetZip` | Ms-PL | https://github.com/haf/DotNetZip.Semverd |
| `FontAwesome.WPF` | MIT | https://github.com/charri/Font-Awesome-WPF |
| `gRPC` (kiosk side, Grpc.Core) | Apache 2.0 | https://github.com/grpc/grpc |
| `Humanizer` | MIT | https://github.com/Humanizr/Humanizer |
| `INIFileParser` | MIT | https://github.com/rickyah/ini-parser |
| `LiteDB` | MIT | https://github.com/mbdavid/LiteDB |
| `LiveCharts.Wpf` | MIT | https://github.com/Live-Charts/Live-Charts |
| `MaterialDesignInXamlToolkit` | MIT | https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit |
| `Microsoft.WindowsAPICodePack` | Microsoft Software License | https://github.com/aybe/Windows-API-Code-Pack-1.1 |
| `NLog` | BSD-3-Clause | https://github.com/NLog/NLog |
| `Newtonsoft.Json` | MIT | https://github.com/JamesNK/Newtonsoft.Json |
| `OpenVR` (`openvr_api.dll`) | BSD-3-Clause | https://github.com/ValveSoftware/openvr |
| `Polly` | New BSD | https://github.com/App-vNext/Polly |
| `QRCoder` | MIT | https://github.com/codebude/QRCoder |
| `Reactivex.io` (System.Reactive) | MIT | https://github.com/dotnet/reactive |
| `SciChart.Wpf.UI` | proprietary | https://www.scichart.com — license file under `LeapVR.Shell.Build/License/` |
| `SharpVectors` | New BSD | https://github.com/ElinamLLC/SharpVectors |
| `SimpleInjector` | MIT | https://github.com/simpleinjector/SimpleInjector |
| `SteamWebAPI2` | MIT | https://github.com/babelshift/SteamWebAPI2 |
| `Unosquare.FFME` (`ffmediaelement`) | Ms-PL | https://github.com/unosquare/ffmediaelement |
| `ValueInjecter` | MIT | https://github.com/omuleanu/ValueInjecter |
| `WPFLocalizeExtension`, `XAMLMarkupExtensions` | MS-PL | https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension |
| `XInputDotNet` | Ms-PL | https://github.com/speps/XInputDotNet |
| `ZKWeb` (FluentValidation derivative) | Apache 2.0 | https://github.com/zkweb-framework/ZKWeb |
| `protobuf-net` | Apache 2.0 | https://github.com/protobuf-net/protobuf-net |
| `soundtouch` (via ffmediaelement) | LGPL-2.1 | https://github.com/Unosquare/soundtouch |

## Native runtime dependencies

| Component | License | Notes |
|---|---|---|
| FFmpeg 4.x (libavcodec/libavformat/libavutil/libswresample/libswscale, win64-shared) | LGPL-2.1+ (defaults) / GPL when compiled with `--enable-gpl` | The kiosk consumes FFmpeg only via the LGPL build. `Build_Free.bat` downloads the LGPL build on demand. If you swap in a GPL build (with x264/x265/etc.) your **redistribution** is governed by GPL terms. https://ffmpeg.org/legal.html |
| Unity-built `vrlounge_desktop.exe` | Unity Personal/Plus/Pro EULA (whichever was used to author it) | Lives at `LeapVR.Shell.3rdParty/bin/vr_desktop/`. The Unity EULA governs redistribution of the binary; we do not redistribute the Unity engine itself. https://unity.com/legal |
| SteamVR runtime | Steam Subscriber Agreement | The kiosk loads OpenVR via Steam at runtime; users must accept Valve's terms separately. https://www.valvesoftware.com/en/openvr-sdk |

## Build tools

| Tool | License | Notes |
|---|---|---|
| Inno Setup 5 | Inno Setup license (closely related to MIT) | Used to produce the kiosk installer. https://jrsoftware.org/files/is5-whatsnew.htm |
| ConfuserEx | MIT | Optional, only used by the non-Free build path. https://github.com/yck1509/ConfuserEx |
| OpenSSL (config templates only) | Apache 2.0 / OpenSSL License | Used to generate development TLS certs via `_Certificates/ssl create/cert-create-all.bat`. We do not redistribute OpenSSL itself. https://www.openssl.org/source/license.html |

## Trademarks

"PlayOnDemand", "LeapPlay", "Steam", "SteamVR", "Unity", "FFmpeg", "Inno Setup", and other product names are property of their respective owners. The presence of a name here does not imply endorsement.
