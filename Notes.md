## Notes

- Naming for Credential Manager
	- VnManager.DbEnc - used for database encryption
		-Username is HASH|SALT
		-Password is the db password
	- VnManager.FileEnc - used for all file encryption
		- File encryption key is a RANDOMIZED 64 character key with 16 non alphanumeric characters