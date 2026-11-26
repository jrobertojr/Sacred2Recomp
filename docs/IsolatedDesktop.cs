using System;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
public static class S2Isolated {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] struct STARTUPINFO { public int cb; public string reserved,desktop,title; public int x,y,xsize,ysize,xchars,ychars,fill,flags; public short show,reserved2; public IntPtr reservedPtr,stdin,stdout,stderr; }
 [StructLayout(LayoutKind.Sequential)] struct PI {public IntPtr process,thread; public uint pid,tid;}
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr CreateDesktopW(string name,IntPtr device,IntPtr devmode,uint flags,uint access,IntPtr security);
 [DllImport("user32.dll",SetLastError=true)] static extern bool CloseDesktop(IntPtr h);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool CreateProcessW(string app,StringBuilder cmd,IntPtr pa,IntPtr ta,bool inherit,uint flags,IntPtr env,string cwd,ref STARTUPINFO si,out PI pi);
 [DllImport("kernel32.dll")] static extern uint WaitForSingleObject(IntPtr h,uint ms);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool TerminateProcess(IntPtr h,uint code);
 [DllImport("kernel32.dll")] static extern bool GetExitCodeProcess(IntPtr h,out uint code);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
 [DllImport("kernel32.dll")] static extern uint SetErrorMode(uint mode);
 public static string Run(string exe,string args,string cwd,int seconds) {
  string name="S2Test_"+Guid.NewGuid().ToString("N");
  IntPtr desktop=CreateDesktopW(name,IntPtr.Zero,IntPtr.Zero,0,0x10000000,IntPtr.Zero);
  if(desktop==IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(),"CreateDesktop failed; no launch");
  PI pi=new PI(); uint old=SetErrorMode(0x8003);
  try {
   STARTUPINFO si=new STARTUPINFO(); si.cb=Marshal.SizeOf(si); si.desktop=name; si.flags=1; si.show=0;
   if(!CreateProcessW(exe,new StringBuilder("\""+exe+"\" "+args),IntPtr.Zero,IntPtr.Zero,false,0x08000000,IntPtr.Zero,cwd,ref si,out pi)) throw new Win32Exception(Marshal.GetLastWin32Error(),"CreateProcess on isolated desktop failed");
   uint wait=WaitForSingleObject(pi.process,(uint)(seconds*1000));
   bool timed=wait==258;
   if(wait!=0){ if(!TerminateProcess(pi.process,124)) throw new Win32Exception(Marshal.GetLastWin32Error()); WaitForSingleObject(pi.process,10000); }
   uint exit; GetExitCodeProcess(pi.process,out exit);
   return "ISOLATED_DESKTOP="+name+" PID="+pi.pid+" TIME_LIMIT_REACHED="+timed+" EXIT="+exit;
  } finally {if(pi.thread!=IntPtr.Zero)CloseHandle(pi.thread); if(pi.process!=IntPtr.Zero)CloseHandle(pi.process); SetErrorMode(old); CloseDesktop(desktop);}
 }
}