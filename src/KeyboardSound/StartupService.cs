using Microsoft.Win32;
namespace KeyboardSound;
public static class StartupService {
 public static void Set(bool enabled) { using var key=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"); if(enabled) key.SetValue("KeyboardSound",$"\"{Environment.ProcessPath}\" --tray"); else key.DeleteValue("KeyboardSound",false); }
 public static bool IsEnabled() { using var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"); return key?.GetValue("KeyboardSound")!=null; }
}
