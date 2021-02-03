## Notes

- Naming for Credential Manager
	- VnManager.DbEnc - used for database encryption 
		-Username is HASH|SALT 
		-Password is the db password 
	- VnManager.FileEnc - used for all file encryption 
		- File encryption key is a RANDOMIZED 64 character key with 16 non alphanumeric characters 
		
		
-Importing
	- ~~Bring back SecureStore for backing up db encryption for standalone mode~~ 
	-make a seperate utility that takes the VnManager-v2 sqlite db, gets the user data table, exports to json
		then imports that json back into the LiteDb database
		
- See about removing Mvvm Dialogs

- Add toast when starting/stopping game
- Add recent games to quicklaunch bar
- minimize to taskbar when game is launched


- Sort games on game grid?
