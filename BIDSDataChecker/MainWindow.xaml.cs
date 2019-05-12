using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TR.BIDSSMemLib;
namespace BIDSDataChecker
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      SMemLib.BIDSSMemChanged += SMemLib_BIDSSMemChanged;
      SMemLib.OpenDChanged += SMemLib_OpenDChanged;
      SMemLib.PanelDChanged += SMemLib_PanelDChanged;
      SMemLib.SoundDChanged += SMemLib_SoundDChanged;
    }
    SMemLib SML = null;
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      SML = new SMemLib();
      SML?.ReadStart(0);
    }

    private void SMemLib_SoundDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      throw new NotImplementedException();
    }

    private void SMemLib_PanelDChanged(object sender, SMemLib.ArrayDChangedEArgs e)
    {
      throw new NotImplementedException();
    }

    private void SMemLib_OpenDChanged(object sender, SMemLib.OpenDChangedEArgs e)
    {
      throw new NotImplementedException();
    }

    private void SMemLib_BIDSSMemChanged(object sender, SMemLib.BSMDChangedEArgs e)
    {
      throw new NotImplementedException();
    }

    private void Window_KeyEv(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          FontSize++;
          break;
        case Key.Down:
          FontSize--;
          break;
      }
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
      SML?.ReadStop(0);
      SML?.Dispose();
    }
  }
}
