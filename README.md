# smartlink_v2

Sample FlexRadio SmartLink V2 WPF client

This sample will show how to connect to a Flex Signature Series radio via SmartLink. 



This project leverages 
- FlexAPI - Copyright 2012-2017 FlexRadio Systems
- jose-jwt - Copyright (c) 2014-2015 dvsekhvalnov
- Newtonsoft.JSON -  Copyright (c) 2007 James Newton-King
- .NET 4.61 - Microsoft


### Build installation steps

1. Clone this repository to a folder called smartlink_v2
2. Create a folder at same level called FlexLib_API
3. Copy the 'Auth0.Windows.dll' file from the SmartSDR V2.4 to the FlexLib_API folder.
4. Create the folder 'v2_4_9' in the FlexLib_API folder then expand the FlexLib_API_v2.4.9.ZIP file to that folder.
5. Use VisualStudio 2017 to open 'MKCMSoftware.SmartLink.sln'; Note VS will need to download the referenced NuGet packages.

```
.../smartlink_v2/<smartlink_v2 repository here!>
.../FlexLib_API/Auth0.Windows.dll
.../FlexLib_API/v2_4_9/<flex api sdk here!>
```


Flex file resources can be downloaded from: https://www.flexradio.com/downloads/

---

My goal was to help others add SmartLink support to their FlexAPI based projects. I hope this helps.

Thank you to Flex Radio Engineering for permitting me to share this sample project.

73, 
Mark W3II

http://www.mkcmsoftware.com/Flex