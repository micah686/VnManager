These are just personal reminders for me to keep track of what I need to fix/add

- Dispose of views and viewmodels/free up memory
- Add Categories to dropdown
- Add right click to vnList
- Load long lists of names into tables, to deal with lists of 1000+ entries.
- Possibly create a custom style for screenshots to set the border from blue to red when NSFW
- Need to work on VnListViewModel for checking edge cases
- Check VnUserList where big images make the list look weird
- add a bool to prevent users from adding a gaame while another is processing.
- reload screenshot viewmodel after changing nsfw status
- Set up method to find and set the Height="" property of the Vns in VnMain, so it adds a proper scrollbar. Currently won't add a scrollbar.
- Set up some sort of wait timer/pause bool to prevent trying to load another Vn once one is selected(for use when someone holds down, aand it tries to load a large amount)


##Entity Framework Notes
I need to install these 2 packages from the nuget package manager(console doesn't work for second one)
```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```