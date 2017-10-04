These are just personal reminders for me to keep track of what I need to fix/add

- [ ] Dispose of views and viewmodels/free up memory
- [ ] Need to work on VnListViewModel for checking edge cases
- [ ] Fix the issue where adding categories keeps triggering the validation helper
- [ ] Check clipboard before allowing user to paste
- [ ] Set up virtualizing lists for large results
- [ ] see about spoilered characters



2:06 PM - $1: The Main and Character tabs have trouble scrolling up using the scroll wheel
2:06 PM - $1: Scrolling down is fine
2:07 PM - $1: The releases page scrolls fine
2:12 PM - $1: The colour theme does not save upon program exit. The colour defaults to light blue when I open the program.
3:25 PM - $1: I don't know if you plan on supporting collection .exes, but I wan't able to add the JAST USA Memorial Collection
3:25 PM - $2: what do you mean?
3:27 PM - $1: The JAST collection has 3 VNs tied to one .exe. I've tried adding it with both the collection vn ID and the IDs of the individual VNs




## Entity Framework Notes
I need to install these 2 packages from the nuget package manager(console doesn't work for second one)
```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Tools
```


### Suggestions from Google Docs
- [ ] Run in taskbar while game is running
- [ ] Automatic updater
- [ ] Vndb Website within program
- [ ] Visual novel scanner to search for visual novels
- [ ] Userlist items with advanced sorting
- [ ] Section tags off by All/Content/ Sexual Content/ Technical content
- [ ] Multiple user profiles?!(probably hard to implement)
- [ ] search for walkthrough/ savegames
- [ ] Password required to do activities(change NSFW settings, set votes/status/…, view any data,....)
- [ ] Add a producers section, like list all Vns from KEY, Nitro+,...
- [ ] Advanced Category filters(sort by relase date,...)
- [ ] Be able to sort visual novels, and name categories, also being able to create sub categories would be nice. Example: sort by translated, or untranslated. Then further sort by producer/genre

### Cleanup notes:
NullImageConverter not used
BindableRichTextBox still exists
VnMainMODEL has some possible null referenceexceptions
Come back to EfEntity
possibly add a template for the statusbar
unload other views?
move as many images to svg/xaml, like in StatusBarViewModel
possibly work on cleaning up VnLinks on Binding
deal with multiple child processes