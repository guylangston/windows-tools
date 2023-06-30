# windows-tools

> TLDR: Get a list of current windows (with handle and class)

```powershell
$ git clone git@github.com:guylangston/windows-tools.git
$ dotnet run | ConvertFrom-Json

 Handle Title                       WindowClass
 ------ -----                       -----------
  65900 Program Manager             Progman
 132160 Windows Input Experience    Windows.UI.Core.CoreWindow
 328434 TweetDeck — Mozilla Firefox MozillaWindowClass
 526848 windows-tools – Program.cs  SunAwtFrame
4852202 Settings                    ApplicationFrameWindow
```


