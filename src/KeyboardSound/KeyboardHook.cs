using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeyboardSound;
public sealed class KeyboardHook:IDisposable {
 private delegate nint HookProc(int code,nint message,nint data);
 private readonly HookProc callback; private nint handle; private readonly HashSet<int> pressed=[];
 private readonly Thread thread; private uint threadId;
 public bool Enabled { get; set; } = true; public bool Repeat { get; set; }
 public event Action<int>? KeyPressed;
 public KeyboardHook() {
  callback=OnKey; using var started=new ManualResetEventSlim(); Exception? error=null;
  thread=new Thread(()=>{threadId=GetCurrentThreadId();handle=SetWindowsHookEx(13,callback,GetModuleHandle(null),0);if(handle==0)error=new Win32Exception(Marshal.GetLastWin32Error());started.Set();if(error!=null)return;System.Windows.Forms.Application.Run();UnhookWindowsHookEx(handle);handle=0;}){IsBackground=true,Name="Keyboard hook"};
  thread.SetApartmentState(ApartmentState.STA);thread.Start();started.Wait();if(error!=null)throw error;
 }
 private nint OnKey(int code,nint message,nint data) {
  if(code>=0) { int key=Marshal.ReadInt32(data); int flags=Marshal.ReadInt32(data,8);
   if((flags&0x10)==0) { if(message==0x101 || message==0x105) pressed.Remove(key);
    if(message==0x100 || message==0x104) { bool first=pressed.Add(key); if(Enabled && (first||Repeat)) { try { KeyPressed?.Invoke(key); } catch { /* Never interrupt the Windows input chain. */ } } }
   }
  } return CallNextHookEx(handle,code,message,data);
 }
 public void Dispose() { if(handle!=0) { PostThreadMessage(threadId,0x12,0,0);thread.Join(2000); } GC.KeepAlive(callback); }
 [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
 [DllImport("user32.dll")] private static extern bool PostThreadMessage(uint thread,uint message,nint w,nint l);
 [DllImport("user32.dll",SetLastError=true)] private static extern nint SetWindowsHookEx(int id,HookProc proc,nint module,uint thread);
 [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(nint hook);
 [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook,int code,nint message,nint data);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] private static extern nint GetModuleHandle(string? name);
}
