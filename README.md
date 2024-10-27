# smartlink v3 (master)

Branch **master** is Sample FlexRadio SmartLink V3 WPF client

This sample will show how to connect to a Flex Signature Series radio via SmartLink. 



This project leverages 
- FlexAPI FlexLib_API_v3.8.19.34216 - Copyright 2012-2024 FlexRadio Systems
- jose-jwt - Copyright (c) 2014-2024 dvsekhvalnov
- Newtonsoft.JSON -  Copyright (c) 2007 James Newton-King
- .NET Framework - Microsoft


### Build installation steps

1. Clone **master** repository to a folder called smartlink
2. Create a folder at same level called FlexLib_API_v3.8.19.34216 by unziping FlexLib to smartlink folder
3. Build referencs 'Auth0.Windows.dll' file from the SmartSDR v2.12.1 installation folder. If you are using SmartSDR V3 then install SmartSDR V2.12.1 without DAX and CAT.
4. Use VisualStudio 2022 to open 'MKCMSoftware.SmartLink.sln'; Note VS will need to download the referenced NuGet packages.

```
.../smartlink/<smartlink repository here!>
.../smartlink/FlexLib_API_v3.8.19.34216
C:\Program Files\FlexRadio Systems\SmartSDR v2.12.1\Auth0.Windows.dll
```




# smartlink v2

Branch  is Sample FlexRadio SmartLink V2 WPF client

This sample will show how to connect to a Flex Signature Series radio via SmartLink. 



This project leverages 
- FlexAPI FlexLib_API_v2.10.1.22964 - Copyright 2012-2024 FlexRadio Systems
- jose-jwt - Copyright (c) 2014-2024 dvsekhvalnov
- Newtonsoft.JSON -  Copyright (c) 2007 James Newton-King
- .NET Framework - Microsoft


### Build installation steps

1. Clone **V2** branch this repository to a folder called smartlink
2. Create a folder at same level called FlexLib_API_v2.10.1.22964 by unziping FlexLib to smartlink folder
3. Build referencs 'Auth0.Windows.dll' file from the SmartSDR v2.12.1 installation folder. If you are using SmartSDR V3 then install SmartSDR V2.12.1 without DAX and CAT.
4. Use VisualStudio 2022 to open 'MKCMSoftware.SmartLink.sln'; Note VS will need to download the referenced NuGet packages.

```
.../smartlink/<smartlink repository here!>
.../smartlink/FlexLib_API_v2.10.1.22964
C:\Program Files\FlexRadio Systems\SmartSDR v2.12.1\Auth0.Windows.dll
```


Flex file resources can be downloaded from: https://www.flexradio.com/downloads/

---

My goal was to help others add SmartLink support to their FlexAPI based projects. I hope this helps.

Thank you to Flex Radio Engineering for permitting me to share this sample project.

I did map FlexLib projects to the .NET Framwork 4.8 SDK in VS 2022.


73, 
Mark W3II

http://www.mkcmsoftware.com/Flex