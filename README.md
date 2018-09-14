## Description
This is a console application that syncronizes the contents of one NuGet feed with another NuGet feed.  The syncronize will only add packages to another feed, and will never remove packages.

## Dependencies
.NET Core 2.0 and above.

## Configuration
The application uses a json file to configure syncronization and chatbot settings.  The json file is in the following format:
```
{
  "replicationPairs": [
    {
      "description": "Replicate Powershell Feeds From ProGet To MyGet",
      "type": "nuget",
      "source": {
        "provider": "proget", 
        "url": "http://nuget.healthwise.org/nuget/Powershell/",
        "token": "TOKEN_WITH_PUSH_CREDENTIALS_TO_PROGET",
        "username": "USER_NAME_WITH_READ_ACCESS_TO_PROGET",
        "password": "PASSWORD_FOR_USER_NAME_WITH_READ_ACCESS_TO_PROGET"
      },
      "destination": {
        "provider": "myget",
        "url": "https://healthwise.myget.org/F/powershellmodules/api/v2",
        "token": "TOKEN_WITH_PUSH_CREDENTIALS_TO_MYGET",
        "username": "USER_NAME_WITH_READ_ACCESS_TO_MYGET",
        "password": "PASSWORD_FOR_USER_NAME_WITH_READ_ACCESS_TO_MYGET"
      }
    }
  ],
    "messageSettings": 
    {
      "FunctionKey": "FUNCTION_KEY",
      "ServiceURL": "https://smba.trafficmanager.net/amer/",
      "ChannelData": "CHANNEL_ID",
      "Environment":  "Dev"
    }
}
```

**Replication Pairs**
An array of items that describe a source feed and a destination feed for copying packages.  A minimum of one replication pair is needed.  For syncronizing multiple feeds, multiple replication pairs will be added to the array.

**Type**
The type of repository to copy.  Can either be *nuget* or *npm*.  The npm functionality is not tested.

**Description**
User friendly description of the replication pair.  This will be written in the log files.

**Source / Destination**
Every replication pair needs to have a single *source* object and a single *destination* object.  The source object is where NuGet packages will be copied *from*.  The destination object is where NuGet packages will be copied *to*.

**Provider**
The type of NuGet installation used.  Valid values are *proget* and *myget*.

**Url**
The URL to the NuGet feed to be replicated.  Refer to the provider documentation on the correct formatting of the URL for that provider.  The *config.sample.json* file has sample urls for the ProGet and MyGet providers.

**Token**
A token with write access to a feed.  For source repositories, the token can be represented by an empty string.  The token must be issued for the same user described in the *Username* / *Password* items.

**Username**
User with read access to a feed.

**Password**
Password associated with the *Username* item.

**MessageSettings**
Describes the properties necessary to send messages to a chat bot service

**FunctionKey**
A function key used to trigger the function with. Keys can be found/created under the 'Manage' blade in the Message Proxy function in the Azure Portal. A new key should be created for each instance where a key will be stored (Azure Key Vault, Azure Automation, etc...)

**ServiceURL**
The URL of the service where messages will be sent

**ChannelData**
Used to deliver messages to specific teams channels. To get a channel ID from Teams ask Uncle HAB for the Channel ID from that Channel. *Example: @Dev-UncleHAB- what is the channel id?* 

**Environment**
The environment where the Uncle HAB message proxy function lives. Valid environments are Dev, Test, and Prod.

The .NET solution contains a `config.sample.json` file that you can use as a template for configuration.

## Running
This program is inteded to be run as a scheduled task but can be run manually as well.  To run, use the following command:
```
dotnet org.healthwise.ops.nugetsync.dll 
```

By default the application will look for a `config.json` file in the same directory as the `org.healthwise.ops.nugetsync.dll`.  To supply an alternate configuration file, supply a file using the `-c` flag:
```
dotnet org.healthwise.ops.nugetsync.dll -c myotherconfig.json
```

## Logging
Output will be logged to the directory containing the `org.healthwise.ops.nugetsync.dll` file named `log-<timestamp>.txt`.  By default a new log file will be created each day, up to 7 files in total.  Older files will then be removed from the directory.