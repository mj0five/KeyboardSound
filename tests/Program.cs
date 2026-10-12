using KeyboardSound;
using NAudio.Wave;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

internal static class Program {
 [STAThread] static int Main(string[] args) {
  var root=Path.Combine(Path.GetTempPath(),"KeyboardSoundTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  int passed=0;
  void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
  try {
   var store=new ProfileStore(root);var s=store.Load();Check(s.Profiles.Count==1,"default profile");
   s.Profiles[0].Keys[65]="sound";s.Profiles.Add(new Profile{Name="Other",Keys=new(s.Profiles[0].Keys)});s.Profiles[1].Keys[65]="other";s.ActiveProfile=1;s.Volume=.4f;store.Save(s);
   var loaded=store.Load();Check(loaded.ActiveProfile==1&&loaded.Profiles[0].Keys[65]=="sound"&&loaded.Profiles[1].Keys[65]=="other"&&loaded.Volume==.4f,"profile isolation and persistence");
   File.WriteAllText(Path.Combine(root,"settings.json"),"broken");Check(store.Load().Profiles.Count==1&&Directory.GetFiles(root,"*.corrupt-*").Length==1,"corrupt settings backup");
   bool rejected=false;try{store.PathFor(new SoundAsset{File="../outside.wav"});}catch{rejected=true;}Check(rejected,"path traversal rejection");
   var wav=Path.Combine(root,"test.wav");using(var writer=new WaveFileWriter(wav,new WaveFormat(22050,16,1))){for(int i=0;i<2205;i++)writer.WriteSample((float)(Math.Sin(i*.12)*.2));}
   var pcm=AudioEngine.Decode(wav);Check(pcm.Length>9000&&pcm.Length<10000&&pcm.All(float.IsFinite),"WAV mono decode and 48 kHz stereo resampling");
   var mp3=Path.Combine(root,"test.mp3");using(var reader=new AudioFileReader(wav))MediaFoundationEncoder.EncodeToMp3(reader,mp3);Check(AudioEngine.Decode(mp3).Length>0,"MP3 decode");
   var longWav=Path.Combine(root,"long.wav");using(var writer=new WaveFileWriter(longWav,new WaveFormat(8000,16,1)))writer.Write(new byte[8000*2*16],0,8000*2*16);
   rejected=false;try{AudioEngine.Decode(longWav);}catch(InvalidDataException){rejected=true;}Check(rejected,"long audio rejection");
   if(args.Contains("--integration")) {
    using(var hook=new KeyboardHook())Check(true,"native hook install and dispose");
    using(var audio=new AudioEngine()){audio.Start();audio.Volume=0;audio.SetSounds(new(){[65]=pcm});for(int i=0;i<100;i++)audio.PlayKey(65);Thread.Sleep(250);Check(true,"audio device and overlapping voices");}
    var app=new App();app.InitializeComponent();var window=new MainWindow(Path.Combine(root,"ui"));window.Show();window.UpdateLayout();
    var bitmap=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,System.Windows.Media.PixelFormats.Pbgra32);bitmap.Render(window);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(Path.GetFullPath("ui-smoke.png")))encoder.Save(file);
    Check(window.ActualWidth>=960,"WPF window layout and rendering");app.Shutdown();
   }
   Console.WriteLine($"Passed {passed} tests");return 0;
  }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
  finally{Directory.Delete(root,true);}
 }
}
