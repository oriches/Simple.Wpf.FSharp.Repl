Simple.Wpf.FSharp.Repl
======================

[![Build status](https://ci.appveyor.com/api/projects/status/kqrptn5shaen1kld?svg=true)]
(https://ci.appveyor.com/project/oriches/simple-wpf-fsharp-repl)

A simple F# REPL engine for use in a WPF application. Mimics the F# Interactive console application inside a WPF user control. Currently based on the open source F# 3.1 Interactive process.

Currently we support the following .Net versions:

Supported versions:

	.NET framework 4.8 and higher,
	
This library is available as a nuget [package] (https://www.nuget.org/packages/Simple.Wpf.FSharp.Repl/).

For more information about the releases see [Release Info] (https://github.com/oriches/Simple.Wpf.FSharp.Repl/wiki/Release-Info).

Example usages of the control, with styles applied dynamically, these were taken from the Wpf.TestHarness project:

![alt text](https://raw.github.com/oriches/Simple.Wpf.FSharp.Repl/master/Readme%20Images/examples.png "Example usage using 2 different themes")

### Code behind implementation

```
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```
XAML:
```
<v:ReplWindow x:Name="ReplWindow" />
```

### MVVM implementation

```
public sealed class MainViewModel
{
    private readonly IReplEngineController _controller;

    public MainViewModel()
    {
        _controller = new ReplEngineController("let ollie = 1337;;");
    }

    public IReplEngineViewModel Content { get { return _controller.ViewModel; } }
}
```

XAML:
```
<v:ReplEngine x:Name="ReplEngine"
              Grid.Row="1"
              DataContext="{Binding Path=Content, Mode=OneWay}"/>
```


### Blog post about developing this control

There is a set of blog posts which detail the journey of creating this control & nuget package - [part 1](http://awkwardcoder.blogspot.co.uk/2013/12/simple-f-repl-in-wpf-part-1.html), [part 2](http://awkwardcoder.blogspot.co.uk/2013/12/simple-f-repl-in-wpf-part-2.html), [part 3](http://awkwardcoder.blogspot.co.uk/2013/12/simple-f-repl-in-wpf-part-3.html) & [part 4] (http://awkwardcoder.blogspot.co.uk/2014/01/simple-f-repl-in-wpf-part-4.html).
