# Selfcontained C# WPF compatible utility classes
__Some classes of my various projects.__
> Note: These classes are self contained. Usually for WPF. If not, they will be marked.

MIT license used.

- `TCP-XML-Stream/`
	- XML Stream functions. Including: .Net Core compatible code, Node Red tester, simple to use
- `Utils MSFT_Helpers.cs`
	- is wrapping the __MSFT_StorageObject__, __MSFT_Partition__ and __MSFT_Disk__ system classes for easy use and using IDE completable properties
- `Utils.Admin.cs`
	- has a check and a method with callback to restart in elevated mode
- `Utils.GetUpdate.cs`
	- simple update checker, download, execute functions - based on server with simple json file - using IDE completable properties
- `Utils.InstalledApplications.cs`
	- checks the registry for a list exe-names of installed applications (or a single one)
- `Utils.ManagementConsoleWatchers.cs`
	- this uses _WqlEventQuery_ to check for an inserted Disk drive (disk found event watcher), but can be used for any system events
- `Utils.ObservableProperties.cs`
	- simple properites that will automagically update the WPF UI if modified (as single properties, as part of your main class)
- `Utils.SingleInstance.cs`
	- Prevent the app from running again. No Mutex required, using Task, no TCP.
- `Utils.StartArgs.cs`
	- creates a dictionary with distinct start params (change it to list for mutliple same params), supporting linux and windows and microsoft param styles equally
- `Utils.StartWithWindows.cs`
	- registers the current app (registry) to be started with windows, or removes it
- `Utils.TcGetMounts.cs`
	- get TrueCrypt mounted drives and their infos
- `Utils.VcGetMounts.cs`
	- get VeraCrypt (newer then TrueCrypt and frequently updated) mounted drives and _some_ of their infos exposed
- `Utils.VcGetMounts.v2.cs`
	- get VeraCrypt (newer then TrueCrypt and frequently updated) mounted drives and _more_ of their infos exposed
  - required by the commandline tool [VeraCrypt-Cmd (GitHub)](https://github.com/BananaAcid/VeraCrypt-Cmd)
- `Utils.WinIcon.cs`
	- get UAC icon, and others as well as any with a generic (in small and large) as Bitmap, using IDE completable properties
- `passing a method.txt`
	- simple explaination on how to call an event on a class
- `simple language resourcedictionaries + usage.txt`
	- simple explaination on how to create language files as XML resources (strings, textblocks, images, ...), with switchable lang, and WPF UI support (no empty areas)
- `using windows styled messageboxes in WPF with compatible syntax.txt`
	- a drop in extension to keep syntax with the WPF MessageBox commands, but using the Windows 8/10 styled ones

more:
- `filterNewsletterEmlsForSpecificProject.cs` [here](https://gist.github.com/BananaAcid/6e60195598e364b1e12e5d4f92d8c9b8)
- `eml2img.cs` [here](https://gist.github.com/BananaAcid/6e60d93921469732bd3d29fd2bd88153)
	- extract images from eml files
