using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TR.BIDSSMemLib;
namespace BIDSDataChecker
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    static int TickRate = 50;
    DispatcherTimer FontSizeShowDT = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 10) };
    DispatcherTimer DT = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, TickRate) };
    DispatcherTimer PDT = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, TickRate) };
    DispatcherTimer SDT = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, TickRate) };
    Run[] PRun = new Run[256];
    Run[] SRun = new Run[256];
    public MainWindow()
    {
      InitializeComponent();
      FontSizeShowDT.Tick += FontSizeShowDT_Tick;
      DT.Tick += DT_Tick;
      PDT.Tick += PDT_Tick;
      SDT.Tick += SDT_Tick;
      for (int i = 0; i < 16; i++)
      {
        PanelGrid.RowDefinitions.Add(new RowDefinition());
        SoundGrid.RowDefinitions.Add(new RowDefinition());
        PanelGrid.ColumnDefinitions.Add(new ColumnDefinition());
        SoundGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }
      for(int i = 0; i < 256; i++)
      {
        TextBlock TBP = new TextBlock();
        Grid.SetRow(TBP, i / 16);
        Grid.SetColumn(TBP, i % 16);
        TBP.Inlines.Add(i.ToString("x") + ":");
        PRun[i] = new Run() { Text = "0" };
        TBP.Inlines.Add(PRun[i]);
        PanelGrid.Children.Add(TBP);
        TextBlock TBS = new TextBlock();
        Grid.SetRow(TBS, i / 16);
        Grid.SetColumn(TBS, i % 16);
        TBS.Inlines.Add(i.ToString("x") + ":");
        SRun[i] = new Run() { Text = "0" };
        TBS.Inlines.Add(SRun[i]);
        SoundGrid.Children.Add(TBS);
      }
    }

    private void PDT_Tick(object sender, EventArgs e)
    {
      PanelD PDNew = SML.Panels;
      for (int i = 0; i < 256; i++)
      {
        MakeRun(ref PRun[i], PDOld.Length > i ? PDOld.Panels[i] : 0, PDNew.Length > i ? PDNew.Panels[i] : 0);
      }
      PDOld = PDNew;
    }
    private void SDT_Tick(object sender, EventArgs e)
    {
      SoundD SDNew = SML.Sounds;
      for (int i = 0; i < 256; i++)
      {
        MakeRun(ref SRun[i], SDOld.Length > i ? SDOld.Sounds[i] : 0, SDNew.Length > i ? SDNew.Sounds[i] : 0);
      }
      SDOld = SDNew;
    }

    bool IsFSLBOpMinusMinus = false;
    int FSLBShowWaitCount = 0;
    private void FontSizeShowDT_Tick(object sender, EventArgs e)
    {
      if (IsFSLBOpMinusMinus)
      {
        if (FSLBShowWaitCount >= 5)
        {
          double FontSizeOpa = FontSizeShowingLB.Opacity;
          FontSizeOpa -= 0.05;
          FontSizeShowingLB.Opacity = FontSizeOpa <= 0 ? 0 : FontSizeOpa;
          if (FontSizeShowingLB.Opacity <= 0)
          {
            FontSizeShowingLB.Visibility = Visibility.Collapsed;
            FSLBShowWaitCount = 0;
            IsFSLBOpMinusMinus = false;
            FontSizeShowDT?.Stop();
          }
        }
        else FSLBShowWaitCount++;
      }
      else
      {
        double FontSizeOpa = FontSizeShowingLB.Opacity;
        FontSizeOpa += 0.1;
        FontSizeShowingLB.Opacity = FontSizeOpa >= 0.9 ? 0.9 : FontSizeOpa;
        if (FontSizeShowingLB.Opacity >= 0.9) IsFSLBOpMinusMinus = true;
      }
    }
    BIDSSharedMemoryData BSMDOld = new BIDSSharedMemoryData();
    OpenD ODOld = new OpenD();
    PanelD PDOld = new PanelD() { Panels = new int[0] };
    SoundD SDOld = new SoundD() { Sounds = new int[0] };
    private void DT_Tick(object sender, EventArgs e)
    {
      BIDSSharedMemoryData BSMDNew = SML.BIDSSMemData;
      OpenD ODNew = SML.OpenData;
      MakeRun(ref BSMDIsEnabled, BSMDOld.IsEnabled, BSMDNew.IsEnabled);
      MakeRun(ref BSMDVersionNum, BSMDOld.VersionNum, BSMDNew.VersionNum);

      MakeRun(ref BSMDSpecB, BSMDOld.SpecData.B, BSMDNew.SpecData.B);
      MakeRun(ref BSMDSpecP, BSMDOld.SpecData.P, BSMDNew.SpecData.P);
      MakeRun(ref BSMDSpecA, BSMDOld.SpecData.A, BSMDNew.SpecData.A);
      MakeRun(ref BSMDSpecJ, BSMDOld.SpecData.J, BSMDNew.SpecData.J);
      MakeRun(ref BSMDSpecC, BSMDOld.SpecData.C, BSMDNew.SpecData.C);

      MakeRun(ref BSMDStateZ, BSMDOld.StateData.Z, BSMDNew.StateData.Z);
      MakeRun(ref BSMDStateV, BSMDOld.StateData.V, BSMDNew.StateData.V);
      MakeRun(ref BSMDStateHH, BSMDOld.StateData.T / 60 / 60 / 1000, BSMDNew.StateData.T / 60 / 60 / 1000);
      MakeRun(ref BSMDStateMM, (BSMDOld.StateData.T / 60 / 1000) % 60, (BSMDNew.StateData.T / 60 / 1000) % 60);
      MakeRun(ref BSMDStateSS, (BSMDOld.StateData.T / 1000) % 60, (BSMDNew.StateData.T / 1000) % 60);
      MakeRun(ref BSMDStateMS, BSMDOld.StateData.T % 1000, BSMDNew.StateData.T % 1000);
      MakeRun(ref BSMDStateBC, BSMDOld.StateData.BC, BSMDNew.StateData.BC);
      MakeRun(ref BSMDStateMR, BSMDOld.StateData.MR, BSMDNew.StateData.MR);
      MakeRun(ref BSMDStateER, BSMDOld.StateData.ER, BSMDNew.StateData.ER);
      MakeRun(ref BSMDStateBP, BSMDOld.StateData.BP, BSMDNew.StateData.BP);
      MakeRun(ref BSMDStateSAP, BSMDOld.StateData.SAP, BSMDNew.StateData.SAP);
      MakeRun(ref BSMDStateI, BSMDOld.StateData.I, BSMDNew.StateData.I);

      MakeRun(ref BSMDHandB, BSMDOld.HandleData.B, BSMDNew.HandleData.B);
      MakeRun(ref BSMDHandP, BSMDOld.HandleData.P, BSMDNew.HandleData.P);
      MakeRun(ref BSMDHandR, BSMDOld.HandleData.R, BSMDNew.HandleData.R);
      MakeRun(ref BSMDHandC, BSMDOld.HandleData.C, BSMDNew.HandleData.C);

      MakeRun(ref OpenDIsEnabled, ODOld.IsEnabled, ODNew.IsEnabled);
      MakeRun(ref OpenDVer, ODOld.Ver, ODNew.Ver);
      MakeRun(ref OpenDRadius, ODOld.Radius, ODNew.Radius);
      MakeRun(ref OpenDCant, ODOld.Cant, ODNew.Cant);
      MakeRun(ref OpenDPitch, ODOld.Pitch, ODNew.Pitch);
      MakeRun(ref OpenDElapTime, ODOld.ElapTime, ODNew.ElapTime);
      MakeRun(ref OpenDSelfBCount, ODOld.SelfBCount, ODNew.SelfBCount);
      MakeRun(ref OpenDSelfBPos, ODOld.SelfBPosition, ODNew.SelfBPosition);
      MakeRun(ref OpenDPreIsEnabled, ODOld.PreTrain.IsEnabled, ODNew.PreTrain.IsEnabled);
      MakeRun(ref OpenDPreLocation, ODOld.PreTrain.Location, ODNew.PreTrain.Location);
      MakeRun(ref OpenDPreDistance, ODOld.PreTrain.Distance, ODNew.PreTrain.Distance);
      MakeRun(ref OpenDPreSpeed, ODOld.PreTrain.Speed, ODNew.PreTrain.Speed);



      BSMDOld = BSMDNew;
      ODOld = ODNew;
    }

    SMemLib SML = null;
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      SML = new SMemLib();
      SML?.ReadStart(0);
      DT?.Start();
      PDT?.Start();
      SDT?.Start();
    }
    bool IsFSLBShowingMode = true;
    bool IsTickRateShowintMode = true;
    private void Window_KeyEv(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          FontSize++;
          if(IsFSLBShowingMode) FSLBShow(0);
          break;
        case Key.Down:
          FontSize--;
          if (IsFSLBShowingMode) FSLBShow(0);
          break;
        case Key.Left:
          TickRate -= TickRate <= 10 ? 0 : 10;
          DT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          PDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          SDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          if (IsTickRateShowintMode) FSLBShow(1);
          break;
        case Key.Right:
          TickRate += 10;
          DT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          PDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          SDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          if (IsTickRateShowintMode) FSLBShow(1);
          break;
        case Key.F:
          IsFSLBShowingMode = !IsFSLBShowingMode;
          break;
        case Key.Q:
          Close();
          break;
      }
    }
    private void FSLBShow(int Mode)
    {
      FontSizeShowDT?.Stop();
      switch (Mode)
      {
        case 0://FontSize
          FontSizeShowingLB.Content = FontSize.ToString();
          FontSizeShowingLB.Background = new SolidColorBrush(Color.FromArgb(55, 00, 00, 00));
          break;
        case 1://Interval
          FontSizeShowingLB.Content = TickRate.ToString();
          FontSizeShowingLB.Background = new SolidColorBrush(Color.FromArgb(55, 0xff, 00, 00));
          break;
      }
      FontSizeShowingLB.Visibility = Visibility.Visible;
      FontSizeShowingLB.Opacity += 0.1;
      IsFSLBOpMinusMinus = false;
      FSLBShowWaitCount = 0;
      FontSizeShowDT?.Start();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      DT?.Stop();
      PDT?.Stop();
      SDT?.Stop();
      SML?.ReadStop(0);
      SML?.Dispose();
      SML = null;
    }

    static Brush Red = new SolidColorBrush(Colors.Red);
    static Brush White = new SolidColorBrush(Colors.White);
    static Brush Black = new SolidColorBrush(Colors.Black);
    static Brush Trans = new SolidColorBrush(Colors.Transparent);
    internal static void MakeRun(ref Run t, object Old, object New)
    {
      if (t != null)
      {
        if (!Equals(Old, New))
        {
          t.Text = New.ToString();
          t.Background = Red;
          t.Foreground = White;
        }
        else
        {
          if (t.Background != Trans) t.Background = Trans;
          if (t.Foreground != Black) t.Foreground = Black;
        }
      }
    }



  }
}
