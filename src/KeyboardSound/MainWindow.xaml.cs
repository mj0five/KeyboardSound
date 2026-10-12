using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Forms=System.Windows.Forms;

namespace KeyboardSound;
public partial class MainWindow:Window {
 private readonly ProfileStore store; private readonly Settings settings; private readonly AudioEngine audio=new();
 private KeyboardHook? hook; private Forms.NotifyIcon? tray; private bool ready,exiting; private int selected=65;
 private readonly Dictionary<int,Button> keys=[]; private readonly Dictionary<string,float[]> cache=[];
 private Profile Current=>settings.Profiles[settings.ActiveProfile];
 public MainWindow(string? dataRoot=null) {
  store=new ProfileStore(dataRoot); InitializeComponent(); Icon=new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/KeyboardSound.ico")); settings=store.Load(); BuildKeyboard(); RefreshProfiles(); RefreshSounds();
  EnabledBox.IsChecked=settings.Enabled; RepeatBox.IsChecked=settings.Repeat; StartupBox.IsChecked=StartupService.IsEnabled(); VolumeSlider.Value=settings.Volume*100; audio.Volume=settings.Volume;
  try { audio.Start(); hook=new KeyboardHook{Enabled=settings.Enabled,Repeat=settings.Repeat}; hook.KeyPressed+=audio.PlayKey; }
  catch(Exception ex) { Status.Text="Audio / keyboard unavailable: "+ex.Message; }
  using(var iconStream=System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/KeyboardSound.ico")).Stream)
   tray=new Forms.NotifyIcon{Icon=new System.Drawing.Icon(iconStream,32,32),Text="Keyboard Sound",Visible=true};
  var menu=new Forms.ContextMenuStrip(); menu.Items.Add("Open",null,(_,_)=>ShowWindow()); menu.Items.Add("Enable / disable sounds",null,(_,_)=>EnabledBox.IsChecked=!(EnabledBox.IsChecked==true)); menu.Items.Add("Exit",null,(_,_)=>Exit()); tray.ContextMenuStrip=menu; tray.DoubleClick+=(_,_)=>ShowWindow();
  Closing+=(_,e)=>{if(!exiting){e.Cancel=true;Hide();}}; ready=true; Reload(); if(store.Warning!=null) Status.Text=store.Warning;
 }
 private void ShowWindow(){Show();WindowState=WindowState.Normal;Activate();}
 private void Exit(){exiting=true;hook?.Dispose();audio.Dispose();tray?.Dispose();System.Windows.Application.Current.Shutdown();}
 private void BuildKeyboard(){
  AddRow([(27,"Esc"),(112,"F1"),(113,"F2"),(114,"F3"),(115,"F4"),(116,"F5"),(117,"F6"),(118,"F7"),(119,"F8"),(120,"F9"),(121,"F10"),(122,"F11"),(123,"F12")]);
  AddRow([(192,"`"),(49,"1"),(50,"2"),(51,"3"),(52,"4"),(53,"5"),(54,"6"),(55,"7"),(56,"8"),(57,"9"),(48,"0"),(189,"-"),(187,"="),(8,"⌫")]);
  AddRow([(9,"Tab"),(81,"Q"),(87,"W"),(69,"E"),(82,"R"),(84,"T"),(89,"Y"),(85,"U"),(73,"I"),(79,"O"),(80,"P"),(219,"["),(221,"]"),(220,"\\")]);
  AddRow([(20,"Caps"),(65,"A"),(83,"S"),(68,"D"),(70,"F"),(71,"G"),(72,"H"),(74,"J"),(75,"K"),(76,"L"),(186,";"),(222,"'"),(13,"Enter")]);
  AddRow([(160,"L⇧"),(90,"Z"),(88,"X"),(67,"C"),(86,"V"),(66,"B"),(78,"N"),(77,"M"),(188,","),(190,"."),(191,"/"),(161,"R⇧")]);
  AddRow([(162,"LCtrl"),(91,"Win"),(164,"LAlt"),(32,"Space"),(165,"RAlt"),(92,"RWin"),(93,"Menu"),(163,"RCtrl")]);
  AddRow([(45,"Ins"),(46,"Del"),(36,"Home"),(35,"End"),(33,"PgUp"),(34,"PgDn"),(37,"←"),(38,"↑"),(40,"↓"),(39,"→")]);
  AddRow([(144,"Num"),(111,"N/"),(106,"N*"),(109,"N-"),(103,"N7"),(104,"N8"),(105,"N9"),(107,"N+"),(100,"N4"),(101,"N5"),(102,"N6")]);
  AddRow([(97,"N1"),(98,"N2"),(99,"N3"),(96,"N0"),(110,"N."),(44,"PrtSc"),(145,"Scroll"),(19,"Pause"),(173,"Mute"),(174,"Vol-"),(175,"Vol+")]);
 }
 private void AddRow((int Code,string Label)[] row){var panel=new StackPanel{Orientation=System.Windows.Controls.Orientation.Horizontal};foreach(var k in row){var b=new Button{Content=k.Label,Tag=k.Code,MinWidth=35,Height=40,Padding=new Thickness(5),Margin=new Thickness(2),ToolTip=$"{k.Label} (VK {k.Code})"};b.Click+=(_,_)=>{selected=k.Code;Paint();};keys[k.Code]=b;panel.Children.Add(b);}KeyboardPanel.Children.Add(panel);}
 private void Paint(){foreach(var pair in keys){pair.Value.Background=new SolidColorBrush((Color)ColorConverter.ConvertFromString(pair.Key==selected?"#7964DE":Current.Keys.ContainsKey(pair.Key)?"#216995":"#273A55"));}SelectedLabel.Text="Key: "+keys[selected].Content;MappingLabel.Text=Current.Keys.TryGetValue(selected,out var id)?"Assigned: "+(settings.Sounds.Find(s=>s.Id==id)?.Name??"Missing sound"):"No sound assigned";}
 private void RefreshProfiles(){bool wasReady=ready;ready=false;ProfileBox.ItemsSource=null;ProfileBox.ItemsSource=settings.Profiles;ProfileBox.SelectedIndex=settings.ActiveProfile;ProfileName.Text=Current.Name;ready=wasReady;}
 private void RefreshSounds(){SoundBox.ItemsSource=null;SoundBox.ItemsSource=settings.Sounds;if(settings.Sounds.Count>0)SoundBox.SelectedIndex=0;}
 private void Save(){store.Save(settings);}
 private void Reload(){var map=new Dictionary<int,float[]>();var failures=new List<string>();long total=0;foreach(var id in Current.Keys.Values.Distinct()){var sound=settings.Sounds.Find(s=>s.Id==id);if(sound==null)continue;try{if(!cache.ContainsKey(id))cache[id]=AudioEngine.Decode(store.PathFor(sound));total+=cache[id].LongLength*4;if(total>256*1024*1024)throw new InvalidDataException("Profile exceeds 256 MB decoded audio.");}catch(Exception ex){cache.Remove(id);failures.Add(sound.Name+": "+ex.Message);}}foreach(var p in Current.Keys)if(cache.TryGetValue(p.Value,out var data))map[p.Key]=data;audio.SetSounds(map);foreach(var id in cache.Keys.Except(Current.Keys.Values).ToArray())cache.Remove(id);Paint();if(failures.Count>0)Status.Text=string.Join(" | ",failures);}
 private void ProfileChanged(object sender,SelectionChangedEventArgs e){if(!ready||ProfileBox.SelectedIndex<0)return;settings.ActiveProfile=ProfileBox.SelectedIndex;ProfileName.Text=Current.Name;Reload();Save();}
 private string? NameInput(){var name=ProfileName.Text.Trim();if(name.Length==0||name.Length>60){Status.Text="Enter a name of 1–60 characters.";return null;}return name;}
 private void CloneProfile(object sender,RoutedEventArgs e){var name=NameInput();if(name==null)return;if(settings.Profiles.Any(p=>p.Name==name))name+=" copy";settings.Profiles.Add(new Profile{Name=name,Keys=new(Current.Keys)});settings.ActiveProfile=settings.Profiles.Count-1;RefreshProfiles();Reload();Save();}
 private void RenameProfile(object sender,RoutedEventArgs e){var name=NameInput();if(name==null)return;Current.Name=name;RefreshProfiles();Save();}
 private void DeleteProfile(object sender,RoutedEventArgs e){if(settings.Profiles.Count==1){Status.Text="Keep at least one profile.";return;}if(MessageBox.Show("Delete this profile?","Delete profile",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;settings.Profiles.RemoveAt(settings.ActiveProfile);settings.ActiveProfile=0;RefreshProfiles();Reload();Save();}
 private void ImportSound(object sender,RoutedEventArgs e){var dialog=new Microsoft.Win32.OpenFileDialog{Filter="Sound effects|*.mp3;*.wav",Multiselect=true};if(dialog.ShowDialog()!=true)return;var errors=new List<string>();foreach(var file in dialog.FileNames){try{AudioEngine.Decode(file);var sound=new SoundAsset{Name=Path.GetFileNameWithoutExtension(file)};sound.File=sound.Id+Path.GetExtension(file).ToLowerInvariant();File.Copy(file,store.PathFor(sound));settings.Sounds.Add(sound);}catch(Exception ex){errors.Add(Path.GetFileName(file)+": "+ex.Message);}}RefreshSounds();Save();Status.Text=errors.Count>0?string.Join(" | ",errors):"Sounds imported. Select a sound and assign it to a key.";}
 private void AssignSound(object sender,RoutedEventArgs e){if(SoundBox.SelectedItem is not SoundAsset s)return;Current.Keys[selected]=s.Id;Reload();Save();}
 private void PreviewSound(object sender,RoutedEventArgs e){if(SoundBox.SelectedItem is SoundAsset s)audio.Play(AudioEngine.Decode(store.PathFor(s)));}
 private void ClearMapping(object sender,RoutedEventArgs e){Current.Keys.Remove(selected);Reload();Save();}
 private void DeleteSound(object sender,RoutedEventArgs e){if(SoundBox.SelectedItem is not SoundAsset s)return;if(MessageBox.Show("Delete this sound and its mappings from all profiles?","Delete sound",MessageBoxButton.YesNo)!=MessageBoxResult.Yes)return;foreach(var p in settings.Profiles)foreach(var key in p.Keys.Where(k=>k.Value==s.Id).Select(k=>k.Key).ToArray())p.Keys.Remove(key);settings.Sounds.Remove(s);cache.Remove(s.Id);Save();File.Delete(store.PathFor(s));RefreshSounds();Reload();}
 private void OptionsChanged(object sender,RoutedEventArgs e){if(!ready)return;settings.Enabled=EnabledBox.IsChecked==true;settings.Repeat=RepeatBox.IsChecked==true;if(hook!=null){hook.Enabled=settings.Enabled;hook.Repeat=settings.Repeat;}Save();Status.Text=settings.Enabled?"Global sounds enabled":"Global sounds paused";}
 private void VolumeChanged(object sender,RoutedPropertyChangedEventArgs<double> e){if(!ready)return;settings.Volume=(float)(VolumeSlider.Value/100);audio.Volume=settings.Volume;Save();}
 private void StartupChanged(object sender,RoutedEventArgs e){try{StartupService.Set(StartupBox.IsChecked==true);settings.StartWithWindows=StartupBox.IsChecked==true;Save();}catch(Exception ex){StartupBox.IsChecked=StartupService.IsEnabled();Status.Text=ex.Message;}}
 private void HideToTray(object sender,RoutedEventArgs e)=>Hide();
}
