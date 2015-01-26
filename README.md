
Elpis is native Windows client for the Pandora Radio music service, implemented in C# and WPF.  

It includes a C# implementation of the Pandora web API, PandoraSharp, which is roughly a port of the API library used in [Pithos](http://kevinmehall.net/p/pithos/), a Linux Pandora Client.

Check out the main client page here: http://elpis.adamhaile.net

## Features
 * View, Sort and Select Stations
 * Play, Pause, Skip Song
 * Cover and Artist Art
 * Thumbs Up, Thumbs Down, Tired of Song
 * Save user credentials and automatic login
 * Automatically play last station at launch
 * System tray notification with song info
 * Minimize/Close to system tray
 * Pause on System Lock
 * Launch pandora.com info page for song, artist, album and station
 * Purchase songs from Amazon
 * Automatically reconnects on session timeout (no more "Are you still listening...")
 * Creating stations
 * Custom Media Key support (Global and Application level)
 * Automatic update check and download within the client
 * Last.FM Scrobbling
 * HTTP Api allowing a user to control Elpis remotely. Here's a remote control called [ElpisRemote](https://github.com/seliver/ElpisRemote)

## Requirements

Elpis will run anywhere that the Microsoft .NET 4.0 Framework will run. In other words, Windows XP SP3, Vista, and Windows 7. The actual hardware requirements are practically negligible, it actually uses less memory than the HTML5 web version running in a modern browser like Chrome.

Afraid you might not have .NET 4.0 or don’t know what it is? No worries, if you are running the Windows 7 SP1 and have Windows Update enabled, you’ve already got it. If not and the installer finds that it’s not installed, it will automatically download and install it for you. It only takes a few minutes.

## Other Links

 * [Pithos](http://kevinmehall.net/p/pithos/) (Linux Gnome client)
 * [Pianobar](http://6xq.net/projects/pianobar/) (Linux command line client)
 * [LPFM](http://lpfm.codeplex.com/) - Elpis uses (and contributes to) LPFM, an open source .NET API for Last.FM
 * [ElpisRemote](https://github.com/seliver/ElpisRemote) - Android Remote Control for Elpis, built with Java on Eclipse.

