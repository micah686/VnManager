These are just random notes by Nikey646, they do not mean anything, other then a method to store information for myself.

## Secure String Research
http://stackoverflow.com/a/819705/6321845 Make the Login slightly safer?
http://stackoverflow.com/a/41192547/6321845 General reading

## Possible Changes?
Maybe change the helper flags on `VndbFlags` from "Full\*" to "All\*Flags" or something similar. "Full\*" just sounds weird, eg "FullUser", while "AllUserFlags" is a bit more natural?

Make VndbConsole/Core interactive in the sense you can pick where to go and what data to get exactly, rather then the current setup?

Should the `VndbFilters` class be moved into another folder, away from the root directory?

## TODO
Change all instances of Int32 where the number cannot go below 0 to UInt64
Change all instances of Int32 where the number can go below 0 to Int64
Update to `IReadOnlyCollection<T>` instead of `T[]`

## Progress
The rough estimate of progress on the .Net Core port is
- [x] Core logic for Sending and Receiving
- [x] Logging In
- [x] Authenticating a User (Completed under a preprocessor)
- [x] Filters (Possibly reworked)
- [x] New Filter System
- [ ] Review Filters
- [x] Flags
- [x] 8/8 "Get" Commands implemented
- [x] 3/3 "Set" Commands implemented
- [x] Error Handling
- [x] Database Dump Handlers
- [ ] Request Options
