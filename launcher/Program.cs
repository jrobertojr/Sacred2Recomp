using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
Application.Run(new Launcher());

sealed class Launcher : Form {
 readonly TextBox source = new() { Dock=DockStyle.Top, ReadOnly=true };
 readonly TextBox log = new() { Dock=DockStyle.Fill, Multiline=true, ReadOnly=true, ScrollBars=ScrollBars.Vertical, BackColor=Color.FromArgb(24,26,30), ForeColor=Color.White };
 readonly ProgressBar progress = new() { Dock=DockStyle.Top, Height=24, Maximum=5 };
 readonly Label status = new() { Dock=DockStyle.Top, Height=32, Text="Select your original game files." };
 readonly Button install = new() { Text="Install / Recompile", AutoSize=true }, play=new(){Text="Play",AutoSize=true,Enabled=false}, cancel=new(){Text="Cancel",AutoSize=true,Enabled=false};
 readonly Button iso=new(){Text="Browse ISO",AutoSize=true}, folder=new(){Text="Browse extracted folder",AutoSize=true};
 readonly string root=FindRoot();
 string? game, exe;
 CancellationTokenSource? cts;
 static string FindRoot() {
  for(var d=new DirectoryInfo(AppContext.BaseDirectory);d!=null;d=d.Parent)
   if(File.Exists(Path.Combine(d.FullName,"Sacred2Recomp_manifest.toml")) || File.Exists(Path.Combine(d.FullName,"work","rexproject","Sacred2Recomp_manifest.toml"))) return d.FullName;
  return AppContext.BaseDirectory;
 }
 public Launcher(){
  Text="Sacred II — Recomp | Alpha"; Width=820; Height=580; MinimumSize=new(650,420); StartPosition=FormStartPosition.CenterScreen;
  BackColor=Color.FromArgb(36,39,45); ForeColor=Color.Gainsboro; Font=new("Segoe UI",11);
  var header=new Label{Text="SACRED II  /  RECOMP",Font=new("Segoe UI",24,FontStyle.Bold),ForeColor=Color.Goldenrod,Dock=DockStyle.Top,Height=75,TextAlign=ContentAlignment.MiddleCenter};
  var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=48}; bar.Controls.AddRange([iso,folder,install,cancel,play]);
  foreach(Button b in bar.Controls){b.FlatStyle=FlatStyle.Flat;b.BackColor=Color.FromArgb(56,60,66);b.ForeColor=Color.White;}
  Controls.Add(log);Controls.Add(progress);Controls.Add(status);Controls.Add(bar);Controls.Add(source);Controls.Add(header);
  iso.Click+=(_,_)=>{using var d=new OpenFileDialog{Filter="Xbox 360 ISO (*.iso)|*.iso"};if(d.ShowDialog()==DialogResult.OK)SelectSource(d.FileName);};
  folder.Click+=(_,_)=>{using var d=new FolderBrowserDialog();if(d.ShowDialog()==DialogResult.OK)SelectSource(d.SelectedPath);};
  install.Click+=async (_,_)=>await Install();
  cancel.Click+=(_,_)=>cts?.Cancel();
  play.Click+=(_,_)=>{try{if(exe==null||game==null)return;var p=new ProcessStartInfo(exe){WorkingDirectory=Path.GetDirectoryName(exe)!,UseShellExecute=false};p.ArgumentList.Add("--title.game_root="+game);p.ArgumentList.Add("--gpu.driver=vulkan");p.ArgumentList.Add("--gpu.vulkan.render_target_path=fsi");var cache=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Sacred2Recomp","cache");Directory.CreateDirectory(cache);p.ArgumentList.Add("--app.cache_root="+cache);p.ArgumentList.Add("--log.level=info");Process.Start(p);}catch(Exception e){MessageBox.Show(e.Message);}};
  FormClosing+=(_,e)=>{if(cts!=null){e.Cancel=true;cts.Cancel();}};
 }
 void SelectSource(string path){source.Text=path;play.Enabled=false;progress.Value=0;}
 void Append(string line){if(!IsDisposed)BeginInvoke((Action)(()=>{log.AppendText(line+Environment.NewLine);}));}
 void Step(int n,string message){progress.Value=n;status.Text=message;Append(message);}
 string Tool(string name){
  var tools=Path.Combine(root,"work","tools");
  if(Directory.Exists(tools)){var match=Directory.EnumerateFiles(tools,name,SearchOption.AllDirectories).FirstOrDefault();if(match!=null)return match;}
  foreach(var dir in (Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator)){var f=Path.Combine(dir.Trim('"'),name);if(File.Exists(f))return f;}
  throw new FileNotFoundException("Required tool not found: "+name);
 }
 async Task Run(string cmd,string cwd,CancellationToken token,params string[] args){
  var info=new ProcessStartInfo(cmd){WorkingDirectory=cwd,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
  foreach(var arg in args)info.ArgumentList.Add(arg);
  using var p=new Process{StartInfo=info};p.OutputDataReceived+=(_,e)=>{if(e.Data!=null)Append(e.Data);};p.ErrorDataReceived+=(_,e)=>{if(e.Data!=null)Append(e.Data);};
  token.ThrowIfCancellationRequested();p.Start();p.BeginOutputReadLine();p.BeginErrorReadLine();
  using var registration=token.Register(()=>{try{if(!p.HasExited)p.Kill(true);}catch{}});
  await p.WaitForExitAsync();p.WaitForExit();token.ThrowIfCancellationRequested();if(p.ExitCode!=0)throw new Exception(Path.GetFileName(cmd)+" failed: "+p.ExitCode);
 }
 async Task Install(){
  if(string.IsNullOrWhiteSpace(source.Text)){MessageBox.Show("Select an ISO or extracted game folder.");return;}
  cts=new();var token=cts.Token;install.Enabled=iso.Enabled=folder.Enabled=play.Enabled=false;cancel.Enabled=true;log.Clear();
  try{
   var template=Directory.Exists(Path.Combine(root,"work","rexproject"))?Path.Combine(root,"work","rexproject"):root;
   var rex=Tool("rexglue.exe");var cmake=Tool("cmake.exe");var clang=Tool("clang++.exe");var ninja=Tool("ninja.exe");
   var sdk=Directory.GetParent(Path.GetDirectoryName(rex)!)!.FullName;
   var workspace=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Sacred2Recomp","installs",Guid.NewGuid().ToString("N"));
   Directory.CreateDirectory(workspace);var project=Path.Combine(workspace,"project");Directory.CreateDirectory(project);
   Step(0,"Validating tools and input...");
   game=source.Text;
   if(File.Exists(game)){var extract=Tool("extract-xiso.exe");var dest=Path.Combine(workspace,"game");Directory.CreateDirectory(dest);await Run(extract,workspace,token,"-x","-d",dest,game);game=dest;}
   foreach(var name in new[]{"default.xex","s2xserver.xex"})if(!File.Exists(Path.Combine(game,name)))throw new Exception("Missing "+name+" in selected game directory.");
   Step(1,"Game files located. Preparing isolated project...");
   File.Copy(Path.Combine(template,"CMakeLists.txt"),Path.Combine(project,"CMakeLists.txt"));
   Directory.CreateDirectory(Path.Combine(project,"src"));foreach(var f in Directory.GetFiles(Path.Combine(template,"src"),"*.cpp"))File.Copy(f,Path.Combine(project,"src",Path.GetFileName(f)));
   var manifest=File.ReadAllText(Path.Combine(template,"Sacred2Recomp_manifest.toml")).Replace("../extracted/sacred2",game.Replace('\\','/'));
   var manifestPath=Path.Combine(project,"Sacred2Recomp_manifest.toml");File.WriteAllText(manifestPath,manifest);
   Step(2,"Generating native C++ (stage 3 of 5)...");await Run(rex,project,token,"codegen",manifestPath);
   Step(3,"Configuring native build (stage 4 of 5)...");var build=Path.Combine(project,"build");
   await Run(cmake,project,token,"-S",project,"-B",build,"-G","Ninja","-DCMAKE_BUILD_TYPE=Release","-DCMAKE_PREFIX_PATH="+sdk,"-DCMAKE_CXX_COMPILER="+clang,"-DCMAKE_MAKE_PROGRAM="+ninja);
   Step(4,"Compiling native executable (stage 5 of 5)...");await Run(cmake,project,token,"--build",build,"--config","Release");
   exe=Directory.EnumerateFiles(build,"Sacred2Recomp.exe",SearchOption.AllDirectories).FirstOrDefault()??throw new Exception("Build did not produce Sacred2Recomp.exe");
   Step(5,"Build complete. Runtime compatibility not yet validated.");play.Enabled=true;
  }catch(OperationCanceledException){status.Text="Cancelled.";}catch(Exception e){status.Text="Installation failed: "+e.Message;Append(e.ToString());}
  finally{cts.Dispose();cts=null;install.Enabled=iso.Enabled=folder.Enabled=true;cancel.Enabled=false;}
 }
}
