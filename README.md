# PreShellFX

**Work in Progress**

PreShellFX is a fullscreen animated splash screen for Windows, currently under development. At this stage, the application only displays a splash screen with a custom logo during system startup. The final goal is to replace the shell, and allow for full customization of the splash screen, and management of startup applications through a configuration panel.

---

## Current Status

Only the basic splash screen is implemented.

✅ Splash screen with animated logo  
❌ No shell replacement yet  
❌ No startup config panel  
❌ No JSON config system integration yet (manual setup required)

---

## Installation Guide (Testing Version)

1. **Download or clone** this repository.
2. Inside the repo, go to the folder:  
   `Assets/Config/`
3. Copy the file:  
   `startupApps.json`
4. Open the Run dialog (`Win + R`), type:  
   `%appdata%` and hit Enter.
5. Inside the `Roaming` folder, create a new folder called:  
   `PreShellFX`
6. Paste the `startupApps.json` file into that folder.
   Your path should now look like this:  
   `C:\Users\YourUsername\AppData\Roaming\PreShellFX\startupApps.json`
7. Edit names and paths to your startup applications in `startupApps.json` file.
8. Now run the application. It should launch without a config error.

---

## Planned Features

- Shell replacement before desktop loads
- Manage and toggle startup apps in splash screen
- Splash screen customization (logo, animation, colors, etc.)
- Auto-create config files during installation
- Installable package with GUI setup panel

---

## Author
Created by **Kalfox**  
*A TailCoded Studio Project*  
<img src="https://storage.googleapis.com/kalfoximg/logos/kalfox-logo.png" alt="Kalfox Logo" width="50" />
<img src="https://storage.googleapis.com/kalfoximg/logos/tailcoded-logo.png" alt="TailCoded Logo" width="50" />

---

## License

This project is **not open-source**.

**You may use** the compiled application for personal testing  
**You may NOT** copy, modify, or reuse the code **without written permission**

See [LICENSE](LICENSE) for full terms.