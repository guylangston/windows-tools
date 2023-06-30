# windows-tools

> TLDR: Get a list of current windows (with handle and class)

```pwsh
git\guy\windows-tools via .NET v7.0.302 🎯 net7.0
❯ dotnet run | ConvertFrom-Json

 Handle Title                       WindowClass
 ------ -----                       -----------
  65900 Program Manager             Progman
 132160 Windows Input Experience    Windows.UI.Core.CoreWindow
 328434 TweetDeck — Mozilla Firefox MozillaWindowClass
 526848 windows-tools – Program.cs  SunAwtFrame
4852202 Settings                    ApplicationFrameWindow
```


