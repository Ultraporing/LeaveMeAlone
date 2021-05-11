# README
![Preview Image](https://repository-images.githubusercontent.com/363516249/ed427880-b2a0-11eb-9566-8bf9bc4218fe)

[![LeaveMeAlone Build & Publish](https://github.com/Ultraporing/LeaveMeAlone/actions/workflows/dotnet-desktop.yml/badge.svg?branch=master)](https://github.com/Ultraporing/LeaveMeAlone/actions/workflows/dotnet-desktop.yml)
[![LeaveMeAlone Code Analyze](https://github.com/Ultraporing/LeaveMeAlone/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/Ultraporing/LeaveMeAlone/actions/workflows/codeql-analysis.yml)
## What is it?
This tool allows you to play GTA5 Online in your own whitelisted session with your Friends, by Blacklisting everyone eles and updating your friends whitelisted IPs in the Firewall everytime you start the program.
It just reads the ```data.json``` file inside the Guardian folder, this file contains the IPs of your friends and updates the Firewall Rules with the new IPs. 

## Important
The Tool requires Admin privileges to Run, since it changes the Windows Firewall Rules. 
And you need to Enable the Firewall for the Rules to take effect.

## How to Use it
1. Can be skipped if you already got an Guardian Account and the Software setup. You need to go to the [DigitalArc (Guardian) Website](https://www.thedigitalarc.com/software/guardian), create an account, install Guardian on your PC, follow the "activate the cloud feature" Guide and lastly add some friends and whitelist them in guardian.
After this is all done, you only have to start Guardian to sync the IPs once your public ip changes, or one of your friends IPs.

2. You can run the Program without any startup parameters to Search the Steamfolders for GTA5. Or you can provide the path to the GTA5.exe if you do not have a Steam version installed.
i.e: ```LeaveMeAlone.exe "H:\SteamLibrary\steamapps\common\Grand Theft Auto V\GTA5.exe"```

3. Once the program finishes, this Tool will Automatically have set the appropriate Windows Firewall Rules to Whitelist your Friends from the DigitalArc Guardian tool and prevent other non friends from joining your session (if you are the host).
