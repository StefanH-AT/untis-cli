# Untis CLI
Little command tool to interact with WebUntis

Uses [this](https://github.com/schletz/UntisLibrary) library to interface with WebUntis.

### Config
A config is needed for the login credentials. The config is loaded from ~/.config/untis-cli/config.json (A template will be generated on first launch)

```
{
  "user": " ", // Your username
  "pass": " ", // Yes, this requires a password. There's no way to interface with api tokens..
  "server": "", // The untis server you wish to connect to. (No https:// or paths, just raw cname). Example: neilo.webuntis.com 
  "schoolName": "" // The school name. Should be the ?school= GET parameter in your webuntis url
}
```

### Caching
A cache is used to store data that does not change very often like teacher, room or subject data. To refresh the cache, run `-f` or `--refresh-cache`. The cache is located in `~/.cache/untis-cli/cache.json`. Run `-v` (`--verbose`) to find out when a connection to untis is made and when the cache is being accessed. This can be used to determine which actions require an untis connection and which don't. 

For example, showing the remaining lesson time (`-r`) does not require a server request and can work just from the cache, as it only accesses the period data. So you can use this command in quick intervals without having to care about network traffic or being timed out.

### Parameters
The CLI parameters can be listed via `-h`
