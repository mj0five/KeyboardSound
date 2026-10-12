using System.IO;
using System.Text.Json;

namespace KeyboardSound;
public sealed class SoundAsset { public string Id { get; set; } = Guid.NewGuid().ToString("N"); public string Name { get; set; } = ""; public string File { get; set; } = ""; public override string ToString() => Name; }
public sealed class Profile { public string Name { get; set; } = "Default"; public Dictionary<int,string> Keys { get; set; } = new(); public override string ToString() => Name; }
public sealed class Settings {
 public List<Profile> Profiles { get; set; } = [new()]; public List<SoundAsset> Sounds { get; set; } = [];
 public int ActiveProfile { get; set; } public float Volume { get; set; } = .65f; public bool Enabled { get; set; } = true;
 public bool Repeat { get; set; } public bool StartWithWindows { get; set; }
}
public sealed class ProfileStore {
 public string Root { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyboardSound");
 public string SoundsPath => Path.Combine(Root,"Sounds");
 public string? Warning { get; private set; }
 public ProfileStore(string? root = null) { if(root != null) Root=root; Directory.CreateDirectory(SoundsPath); }
 public Settings Load() {
  var file=Path.Combine(Root,"settings.json"); if(!File.Exists(file)) return new();
  try { var s=JsonSerializer.Deserialize<Settings>(File.ReadAllText(file)) ?? throw new InvalidDataException();
   if(s.Profiles == null || s.Sounds == null || s.Profiles.Count == 0 || s.Profiles.Any(p=>p==null || p.Keys==null)) throw new InvalidDataException();
   s.ActiveProfile=Math.Clamp(s.ActiveProfile,0,s.Profiles.Count-1); s.Volume=float.IsFinite(s.Volume)?Math.Clamp(s.Volume,0,1):.65f; return s;
  } catch(Exception ex) when(ex is JsonException or IOException or InvalidDataException) { File.Copy(file,file+".corrupt-"+DateTime.Now.ToString("yyyyMMddHHmmss"),true); Warning="Settings were damaged. A backup was preserved; defaults loaded."; return new(); }
 }
 public void Save(Settings s) { var file=Path.Combine(Root,"settings.json"); File.WriteAllText(file+".tmp",JsonSerializer.Serialize(s,new JsonSerializerOptions{WriteIndented=true})); File.Move(file+".tmp",file,true); }
 public string PathFor(SoundAsset sound) { if(Path.GetFileName(sound.File)!=sound.File) throw new InvalidDataException("Invalid sound path"); return Path.Combine(SoundsPath,sound.File); }
}
