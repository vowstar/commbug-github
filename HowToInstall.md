# Introduction:How to install commbug? #

以下将要介绍Commbug的安装方法.

---

# 详细安装方法: #

### Linux下 ###
> ### Ubuntu下 ###
> #### Ubuntu 10.04 和 10.10 下 ####

> 打开终端,
```
   sudo add-apt-repository ppa:huangr08/ppa

   sudo apt-get update

   sudo apt-get install commbug
```


> #### Ubuntu 8.04 下 ####
```
   sudo apt-key adv –keyserver keyserver.ubuntu.com –recv-keys D4A1DA23 
```

> 在/etc/apt/sources.list中添加:
```

   deb http://ppa.launchpad.net/huangr08/ppa/ubuntu hardy main 

   deb-src http://ppa.launchpad.net/huangr08/ppa/ubuntu hardy main

```

> 然后
```
   sudo apt-get update

   sudo apt-get install commbug
```


> #### 其他发行版下: ####

  1. 下载最新的源码包
```
   commbug-latest.tar.gz
```
> [到此下载](http://code.google.com/p/commbug/downloads/list)
> 注:本文中使用的latest,要根据您实际使用的版本而定.
> 2.安装编译所依赖的库和软件包
```
   autotools-dev, 

   mono-devel,  

   libglade2.0-cil-dev,   

   libmono-addins-cil-dev (>= 0.3.1),

   libmono-addins-gui-cil-dev (>= 0.3.1),

   libglib2.0-cil-dev,

   libgtk2.0-cil-dev (>= 2.12), 

   gconf2,

   libglib2.0-dev,

   libgtk2.0-dev (>= 2.8)
```
> 3.编译安装
```
   tar -zxvf commbug-latest.tar.gz

   cd commbug-latest

   ./configure

   make

   sudo make install 
```
> 4.卸载编译所依赖的库和软件包,安装运行所依赖的库和软件包,以便您获得最干净的系统.
```
   mono

   libmono-i18n2.0-cil
```
> 5.终端运行
```
   commbug
```
### Windows下: ###

> ### Windows 2000,XP ###
    1. 下载并且安装[.NET Framework 2.0](http://msdn.microsoft.com/en-us/netframework/aa731542.aspx)或者[.NET Framework 3.5](http://www.microsoft.com/downloads/details.aspx?displaylang=en&FamilyID=c17ba869-9671-4330-a63e-1fd44e0e2505)(当然3.5更好).
    1. 到 http://ftp.novell.com/pub/mono/gtk-sharp/ 下载最新的gtk-sharp并且安装.[下载gtk-sharp-2.12.9-2.win32.msi](http://ftp.novell.com/pub/mono/gtk-sharp/gtk-sharp-2.12.9-2.win32.msi)
    1. 下载[commbug.exe](http://code.google.com/p/commbug/downloads/list)并且运行.
> ### Windows Vista ###
    1. 先尝试Windows7的安装方法.
    1. 若上一步失败,尝试XP下的安装方法.
> ### Windows 7 ###
    1. 到 http://ftp.novell.com/pub/mono/gtk-sharp/ 下载最新的gtk-sharp并且安装.[下载gtk-sharp-2.12.9-2.win32.msi](http://ftp.novell.com/pub/mono/gtk-sharp/gtk-sharp-2.12.9-2.win32.msi)
    1. 下载[commbug.exe](http://code.google.com/p/commbug/downloads/list)并且运行.

### MAC OS下 ###
> 开发中