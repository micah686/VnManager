# VndbSharp
A C# [Vndb](https://vndb.org/) API Library. #OriginalNamingScheme

VndbSharp is a C# library for the Visual Novel Database API
  - Supports TLS (Secure Connections)
  - Support for most filters and flags at this time (Jan 2017, Api version 2.27)
  - Easily Extensible

## Usage
Check out the [VndbConsole projects](VndbConsoleCore/Program.cs) for basic usage guidance. The one `Program.cs` file is used for both the .Net Framework and .Net Core projects, but works in both. This shows that the library can be used from either the .Net Framework (4.6+) or .Net Core (.Net Standard 1.3+)

## Building
To build the project you need to use Visual Studio 2017 RC3 or newer.

## Supported Filters
 - Id
 - Alias Id
 - User Id
 - First Character (Letter)
 - Released
 - Language(s)
 - Original Name
 - Original Language(s)
 - Platform(s)
 - Search
 - Tag(s)
 - Trait(s)
 - Title
 - Name 
 - Username
 - Visual Novel

## TODO
There is no guarantee that anything listed on this will be implemented.

  - [ ] Work on the Read Me
  - [ ] Provide a default RequestOptions class
  - [ ] ~~Wiki everything!~~ Document everything (public)!
  - [ ] Nuget package?
