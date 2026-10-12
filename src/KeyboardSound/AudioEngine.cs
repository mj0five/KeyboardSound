using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace KeyboardSound;
public sealed class AudioEngine : IDisposable {
 public static readonly WaveFormat Format=WaveFormat.CreateIeeeFloatWaveFormat(48000,2);
 private readonly MixingSampleProvider mixer=new(Format){ReadFully=true};
 private readonly VolumeSampleProvider volume;
 private WaveOutEvent? output;
 private Dictionary<int,float[]> sounds=new();
 public AudioEngine() { volume=new VolumeSampleProvider(new Limited(mixer)); }
 public void Start() { output=new WaveOutEvent{DesiredLatency=40,NumberOfBuffers=3}; output.Init(volume); output.Play(); }
 public float Volume { set=>volume.Volume=Math.Clamp(value,0,1); }
 public void SetSounds(Dictionary<int,float[]> value)=>Interlocked.Exchange(ref sounds,value);
 public void PlayKey(int key) { if(sounds.TryGetValue(key,out var samples)) Play(samples); }
 public void Play(float[] samples) { if(output==null) return; if(mixer.MixerInputs.Count()>=64) return; mixer.AddMixerInput(new Voice(samples)); }
 public static float[] Decode(string file) {
  if(new FileInfo(file).Length>20*1024*1024) throw new InvalidDataException("Maximum file size is 20 MB.");
  using var reader=new AudioFileReader(file);
  if(reader.TotalTime.TotalSeconds>15) throw new InvalidDataException("Use a sound effect of 15 seconds or less.");
  ISampleProvider source=reader;
  if(source.WaveFormat.Channels==1) source=new MonoToStereoSampleProvider(source);
  if(source.WaveFormat.Channels!=2) throw new InvalidDataException("Only mono and stereo audio is supported.");
  if(source.WaveFormat.SampleRate!=48000) source=new WdlResamplingSampleProvider(source,48000);
  var samples=new List<float>(); var buffer=new float[8192]; int n;
  while((n=source.Read(buffer,0,buffer.Length))>0) { samples.AddRange(buffer.Take(n)); if(samples.Count>48000*2*15) throw new InvalidDataException("Sound exceeds 15 seconds."); }
  if(samples.Count==0) throw new InvalidDataException("Audio file is empty."); return samples.ToArray();
 }
 public void Dispose() { output?.Stop(); output?.Dispose(); }
 private sealed class Voice(float[] data):ISampleProvider { private int position; public WaveFormat WaveFormat=>Format; public int Read(float[] buffer,int offset,int count) { var n=Math.Min(count,data.Length-position); Array.Copy(data,position,buffer,offset,n); position+=n; return n; } }
 private sealed class Limited(ISampleProvider source):ISampleProvider { public WaveFormat WaveFormat=>source.WaveFormat; public int Read(float[] b,int o,int c) { int n=source.Read(b,o,c); for(int i=o;i<o+n;i++) b[i]=Math.Clamp(b[i],-1f,1f); return n; } }
}
