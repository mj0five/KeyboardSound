# Keyboard Sound V1 — validation report

Date: 2026-09-26

## Delivered

- `KeyboardSound-Setup.exe`: Inno Setup per-user installer, approximately 51.2 MiB.
- `KeyboardSound-Source.zip`: source, tests, build script, installer script, README and license notices.
- `KeyboardSound/publish/`: complete self-contained win-x64 release folder.
- `KeyboardSound-Preview.png`: WPF rendering of the main window.

Build tools: .NET SDK 10.0.401, bundled runtime 10.0.12, NAudio 2.2.1, Inno Setup 6.7.3. Final Release publish succeeded without compiler warnings or errors. The official Inno Setup compiler download's Authenticode signature was verified as valid (Pyrsys B.V.). The generated application and installer are unsigned.

## Automated checks passed

1. Initial default profile creation.
2. Independent cloned profile mappings, active profile and volume persisted through JSON save/load.
3. Corrupt settings preserved as a backup and defaults recovered.
4. Path traversal rejected for library files.
5. Mono 22.05 kHz WAV decoded/resampled into 48 kHz stereo PCM.
6. MP3 encoded using Windows Media Foundation and successfully decoded.
7. Audio longer than 15 seconds rejected.
8. Actual Windows low-level hook installed and disposed.
9. Actual output device opened at zero volume; 100 overlapping playback requests completed without error.
10. Actual WPF main window created, laid out and rendered. Screenshot inspected; combo text contrast and panel scrolling corrected and rechecked.

## Installer smoke test passed

The final installer was run silently in an isolated workspace installation directory with Start Menu/Desktop shortcut options. Installation exited successfully, bundled `coreclr.dll` was present, and installed EXE SHA-256 matched the published EXE. The installed application was launched with `--tray` and an invalid external `DOTNET_ROOT`; it remained running. That test process was stopped and the generated uninstaller exited successfully and removed the installed application executable.

This verifies packaging and startup on the development machine. It is not a clean-VM or cross-version Windows certification.

## Remaining manual acceptance checks

- Physical keyboard presses in multiple applications, audible overlap and perceived latency on the target audio hardware.
- Held-key repeat and different keyboard layouts/Num Lock behavior.
- Start with Windows after an actual sign-out/sign-in.
- Fresh Windows 10 and Windows 11 x64 installation, different DPI settings and devices.
- Code signing before broad public distribution.

Secure-desktop/elevated-app input and automatic audio device recovery are not promised by this V1. The installer requires Windows 10 build 19041 or newer. The README lists detailed feature limits and data paths.
