# Narakeet Video Build API example in C#

This repository provides a quick example demonstrating how to access the Narakeet [markdown to video API](https://www.narakeet.com/docs/automating/rest/) from CSharp/.Net Core.

The example sends a request to generate a video from a local ZIP file (it creates the zip file from the contents of the [video](video) directory, then downloads the resulting video into a local temporary file. 

## Prerequisites

This example works with .NET Core 6.0. You can run it inside Docker (then it does not require a local .NET Core installation), or on a system with a .NET Core 6.0 compatible installation.

## Running the example

Set a local environment variable called `NARAKEET_API_KEY`, containing your API key (or modify [NarakeetExample/Program.cs](NarakeetExample/Program.cs) line 13 to include your API key).

Optionally edit [NarakeetExample/Program.cs](NarakeetExample/Program.cs) and modify the video file directory, main video file, and the function that handles progress notification (lines 8, 9 and 30).

Without Docker:

```
dotnet publish -c Release
dotnet run --project NarakeetExample
```

On a system with Docker and GNU Makefile

```
make init
make execute
```

## More information

Check out <https://www.narakeet.com/docs/automating/rest/> for more information on the Narakeet Markdown to Video API. 

