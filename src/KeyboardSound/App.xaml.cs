using System.Windows;
namespace KeyboardSound;
public partial class App:System.Windows.Application {
 private Mutex? mutex;
 protected override void OnStartup(StartupEventArgs e) {
  base.OnStartup(e); mutex=new Mutex(true,"Local\\KeyboardSound.V1",out bool first);
  if(!first) { MessageBox.Show("Keyboard Sound is already running. Open it from the system tray."); Shutdown(); return; }
  DispatcherUnhandledException+=(s,args)=>{ MessageBox.Show(args.Exception.Message,"Keyboard Sound"); args.Handled=true; };
  var window=new MainWindow(); MainWindow=window; if(!e.Args.Contains("--tray")) window.Show();
 }
 protected override void OnExit(ExitEventArgs e) { mutex?.Dispose(); base.OnExit(e); }
}
