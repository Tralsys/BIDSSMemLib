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
        TBP.Inlines.Add(i.ToString() + ":");
        PRun[i] = new Run() { Text = "0" };
        TBP.Inlines.Add(PRun[i]);
        PanelGrid.Children.Add(TBP);
        TextBlock TBS = new TextBlock();
        Grid.SetRow(TBS, i / 16);
        Grid.SetColumn(TBS, i % 16);
        TBS.Inlines.Add(i.ToString() + ":");
        SRun[i] = new Run() { Text = "0" };
        TBS.Inlines.Add(SRun[i]);
        SoundGrid.Children.Add(TBS);
      }
    }

    private void PDT_Tick(object sender, EventArgs e)
    {
      int[] PDNew = StaticSMemLib.ReadPanel();
      for (int i = 0; i < 256; i++)
      {
        MakeRun(ref PRun[i], PDOld.Length > i ? PDOld[i] : 0, PDNew.Length > i ? PDNew[i] : 0);
      }
      PDOld = PDNew;
    }
    private void SDT_Tick(object sender, EventArgs e)
    {
      int[] SDNew = StaticSMemLib.ReadSound();
      for (int i = 0; i < 256; i++)
      {
        MakeRun(ref SRun[i], SDOld.Length > i ? SDOld[i] : 0, SDNew.Length > i ? SDNew[i] : 0);
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
    int[] PDOld = new int[0];
    int[] SDOld = new int[0];
    TimeSpan TSOld = new TimeSpan(0);
    private void DT_Tick(object sender, EventArgs e)
    {
      BIDSSharedMemoryData BSMDNew = StaticSMemLib.ReadBSMD();
      OpenD ODNew = StaticSMemLib.ReadOpenD();
      TimeSpan TSNew = TimeSpan.FromMilliseconds(BSMDNew.StateData.T);

      MakeRun(ref BSMDVersionNum, BSMDOld.VersionNum, BSMDNew.VersionNum);

      MakeRun(ref BSMDSpecB, BSMDOld.SpecData.B, BSMDNew.SpecData.B);
      MakeRun(ref BSMDSpecP, BSMDOld.SpecData.P, BSMDNew.SpecData.P);
      MakeRun(ref BSMDSpecA, BSMDOld.SpecData.A, BSMDNew.SpecData.A);
      MakeRun(ref BSMDSpecJ, BSMDOld.SpecData.J, BSMDNew.SpecData.J);
      MakeRun(ref BSMDSpecC, BSMDOld.SpecData.C, BSMDNew.SpecData.C);

      MakeRun(ref BSMDStateZ, BSMDOld.StateData.Z, BSMDNew.StateData.Z);
      MakeRun(ref BSMDStateV, BSMDOld.StateData.V, BSMDNew.StateData.V);
      MakeRun(ref BSMDStateHH, TSOld.Hours, TSNew.Hours);
      MakeRun(ref BSMDStateMM, TSOld.Minutes, TSNew.Minutes);
      MakeRun(ref BSMDStateSS, TSOld.Seconds, TSNew.Seconds);
      MakeRun(ref BSMDStateMS, TSOld.Milliseconds, TSNew.Milliseconds);
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
      TSOld = TSNew;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      StaticSMemLib.Begin(false, true);
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
          if (FontSize > 1) FontSize--;//FontSizeは最小でも1.
          if (IsFSLBShowingMode) FSLBShow(0);
          break;
        case Key.Left:
          TickRate -= TickRate > 10 ? 10 ://TickRateが10より大きいなら10msごと.
            TickRate > 1 ? 1 : 0;//TickRateが10より小さいなら, 1msより大きい場合に限って1msずつ引く.  1ms未満にはしない.
          DT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          PDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          SDT.Interval = new TimeSpan(0, 0, 0, 0, TickRate);
          if (IsTickRateShowintMode) FSLBShow(1);
          break;
        case Key.Right:
          TickRate += TickRate < 10 ? 1 : 10;//TickRateが10未満なら1msずつ, 10以上なら10msずつ上昇させる.
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
    }

    static readonly Brush Red = new SolidColorBrush(Colors.Red);
    static readonly Brush Green = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0xFF, 0));
    static readonly Brush Blue = new SolidColorBrush(Colors.Blue);
    static readonly Brush White = new SolidColorBrush(Colors.White);
    static readonly Brush Black = new SolidColorBrush(Colors.Black);
    static readonly Brush Trans = new SolidColorBrush(Colors.Transparent);

    static readonly Brush ValueDown = Green;
    static readonly Brush ValueUp = Red;
    internal static void MakeRun<T>(ref Run t, in T Old, in T New) where T : IComparable
    {
      if (t != null)
      {
        if (!Equals(Old, New))
        {
          t.Text = New.ToString();
          t.Background = Old.CompareTo(New) > 0 ? ValueDown : ValueUp;
          //t.Foreground = White;
        }
        else
        {
          if (t.Background != Trans) t.Background = Trans;
          //if (t.Foreground != Black) t.Foreground = Black;
        }
      }
    }



  }
}
