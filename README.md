# Untis CLI
Little command tool to interact with WebUntis

Uses [this](https://github.com/schletz/UntisLibrary) library to interface with WebUntis.

### Config
A config is needed for the login credentials. Default path is `./config.json`, but this can be changed through the `--config` argument. 

```
{
  "user": " ", // Your username
  "pass": " ", // Yes, this requires a password. There's no way to interface with api tokens..
  "server": "", // The untis server you wish to connect to. (No https:// or paths, just raw cname). Example: neilo.webuntis.com 
  "schoolName": "" // The school name. Should be the ?school= GET parameter in your webuntis url
}
```

### Parameters
The CLI parameters can be listed via `-h`
