These are just personal reminders for me to keep track of what I need to fix/add

- Dispose of views and viewmodels/free up memory
- Load long lists of names into tables, to deal with lists of 1000+ entries.
- Possibly create a custom style for screenshots to set the border from blue to red when NSFW
- Need to work on VnListViewModel for checking edge cases
- Check VnUserList where big images make the list look weird
- add a bool to prevent users from adding a gaame while another is processing.
- reload screenshot viewmodel after changing nsfw status
- Set up some sort of wait timer/pause bool to prevent trying to load another Vn once one is selected(for use when someone holds down, aand it tries to load a large amount)
- Prevent user from playing a game while playing another one
- ADD REMOVE GAME TO RIGHT CLICK CONTEXT MENU


##Entity Framework Notes
I need to install these 2 packages from the nuget package manager(console doesn't work for second one)
```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```


###Suggestions from Google Docs
- Run in taskbar while game is running
- Automatic updater
- Vndb Website within program
- Visual novel scanner to search for visual novels
- Userlist items with advanced sorting
- Section tags off by All/Content/ Sexual Content/ Technical content
- Multiple user profiles?!(probably hard to implement)
- search for walkthrough/ savegames
- Password required to do activities(change NSFW settings, set votes/status/…, view any data,....)
- Add a producers section, like list all Vns from KEY, Nitro+,...
- Advanced Category filters(sort by relase date,...)
-Be able to sort visual novels, and name categories, also being able to create sub categories would be nice. Example: sort by translated, or untranslated. Then further sort by producer/genre
