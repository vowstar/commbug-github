using System;
using System.IO.Ports;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	#region 变量与类定义
	public enum ConvertMode
	{
		Text,
		Hex,
		Dec
	}
	public string SplitString = "\t";
	public ConvertMode SendMode = ConvertMode.Text;
	public ConvertMode NowSendMode = ConvertMode.Text;
	public SerialPortEx MyPort;
	private System.Timers.Timer SendTimer;
	private string portName = "";
	private int baudRate = 1200;
	private Parity parity = Parity.None;
	private int dataBits = 0;
	private StopBits stopBits = StopBits.One;
	private CommBug.AboutWindow aboutWindow;
	#endregion
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		textviewSend.Buffer.Changed += HandleTextviewSendBufferChanged;
		#region 初始化串口
		// 初始化串口
		foreach (var Name in System.IO.Ports.SerialPort.GetPortNames ()) {
			comboboxPortName.AppendText (Name);
		}
		int index;
		index = SerialPort.GetPortNames ().Length;
		
		if (index > 0) {
			comboboxPortName.Active = index - 1;
			// 自动选择串口
		} else {
			// 无串口
		}
		SettingsSynchronization ();
		MyPort = new SerialPortEx (portName, baudRate, parity, dataBits, stopBits);
		MyPort.DataReceived += new SerialDataReceivedEventHandler (HandleMyPortDataReceived);
		Console.WriteLine ("Port initializated");
		#endregion
		#region 初始化发送模式
		// 初始化发送模式
		switch (SendMode) {
		case ConvertMode.Text:
			radiobuttonText.Active = true;
			break;
		case ConvertMode.Hex:
			radiobuttonHex.Active = true;
			break;
		case ConvertMode.Dec:
			radiobuttonDec.Active = true;
			break;
		}
		#endregion
		#region 初始化发送定时器
		// 初始化发送定时器
		SendTimer = new System.Timers.Timer ();
		SendTimer.Elapsed += HandleSendTimerElapsed;
		SendTimer.Interval = Convert.ToDouble (spinbuttonInterval.Text);
		SendTimer.AutoReset = true;
		SendTimer.Enabled = false;
		// 自动复位定时器，一直能触发
		#endregion
		
	}


	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		SendTimer.Enabled = false;
		SendTimer.Dispose ();
		MyPort.Dispose ();
		Application.Quit ();
		a.RetVal = true;
	}

	void HandleTextviewSendBufferChanged (object sender, EventArgs e)
	{
		if (NowSendMode != ConvertMode.Text) {
			textviewSend.Buffer.Changed -= HandleTextviewSendBufferChanged;
			string tempstr = textviewSend.Buffer.Text;
			int position = textviewSend.Buffer.CursorPosition + 1;
			
			switch (NowSendMode) {
			case ConvertMode.Hex:
				tempstr = System.Text.RegularExpressions.Regex.Replace (tempstr, "(?<=([0-9A-Fa-f]{2,}))", SplitString);
				tempstr = System.Text.RegularExpressions.Regex.Replace (tempstr, "[^0-9^A-F^a-f]+", SplitString);
				break;
			case ConvertMode.Dec:
				tempstr = System.Text.RegularExpressions.Regex.Replace (tempstr, "[^0-9]+", SplitString);
				tempstr = System.Text.RegularExpressions.Regex.Replace (tempstr, "((?<=(25))(?=[6-9]))|(?<=([^0-9]{0,1}[3-9][0-9]))|(?<=([^0-9]{0,1}2[6-9]))|(?<=([0-9]{3,}))", SplitString);
				tempstr = System.Text.RegularExpressions.Regex.Replace (tempstr, "[^0-9]+", SplitString);
				break;
			}
			
			textviewSend.Buffer.Text = tempstr;
			if (position > textviewSend.Buffer.EndIter.Offset)
				position = textviewSend.Buffer.EndIter.Offset;
			textviewSend.Buffer.PlaceCursor (textviewSend.Buffer.GetIterAtOffset (position));
			textviewSend.Buffer.Changed += HandleTextviewSendBufferChanged;
		}
	}

	#region In Threading Envent
	void HandleSendTimerElapsed (object sender, System.Timers.ElapsedEventArgs e)
	{
		Gdk.Threads.Enter ();
		OnButtonSendClicked (this, null);
		Gdk.Threads.Leave ();
	}

	void HandleMyPortDataReceived (object sender, SerialDataReceivedEventArgs e)
	{
		byte[] buffer = new byte[MyPort.BytesToRead];
		MyPort.Read (buffer, 0, buffer.Length);
		Gdk.Threads.Enter ();
		// 准备在线程中更新界面
		TextIter iter;
		iter = textviewText.Buffer.EndIter;
		textviewText.Buffer.Insert (ref iter, StringConverts.BytesToString (buffer));
		if (checkbuttonAutoScrollReceive.Active) {
			textviewText.Buffer.CreateMark ("EndMark", iter, false);
			textviewText.ScrollToMark (textviewText.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
			textviewText.Buffer.DeleteMark ("EndMark");
		}
		iter = textviewHex.Buffer.EndIter;
		textviewHex.Buffer.Insert (ref iter, StringConverts.BytesToHexString (buffer));
		if (checkbuttonAutoScrollReceive.Active) {
			textviewHex.Buffer.CreateMark ("EndMark", iter, false);
			textviewHex.ScrollToMark (textviewHex.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
			textviewHex.Buffer.DeleteMark ("EndMark");
		}
		iter = textviewDec.Buffer.EndIter;
		textviewDec.Buffer.Insert (ref iter, StringConverts.BytesToDecString (buffer));
		if (checkbuttonAutoScrollReceive.Active) {
			textviewDec.Buffer.CreateMark ("EndMark", iter, false);
			textviewDec.ScrollToMark (textviewDec.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
			textviewDec.Buffer.DeleteMark ("EndMark");
		}
		Gdk.Threads.Leave ();
	}
	#endregion


	private void SettingsSynchronization ()
	{
		int index;
		portName = comboboxPortName.ActiveText;
		Console.WriteLine (portName);
		baudRate = Convert.ToInt32 (comboboxentryBaudRate.ActiveText);
		Console.WriteLine (baudRate);
		index = comboboxPatity.Active;
		switch (index) {
		case 0:
			parity = Parity.None;
			break;
		case 1:
			parity = Parity.Even;
			break;
		case 2:
			parity = Parity.Odd;
			break;
		case 3:
			parity = Parity.Mark;
			break;
		case 4:
			parity = Parity.Space;
			break;
		default:
			parity = Parity.None;
			break;
		}
		Console.WriteLine (parity);
		dataBits = Convert.ToInt32 (spinbuttonDataBits.Text);
		Console.WriteLine (dataBits);
		index = comboboxStopBits.Active;
		switch (index) {
		case 0:
			stopBits = StopBits.None;
			break;
		case 1:
			stopBits = StopBits.One;
			break;
		case 2:
			stopBits = StopBits.OnePointFive;
			break;
		case 3:
			stopBits = StopBits.Two;
			break;
		default:
			stopBits = StopBits.One;
			break;
		}
		Console.WriteLine (stopBits);
		if (MyPort != null) {
			MyPort.PortName = portName;
			MyPort.BaudRate = baudRate;
			MyPort.Parity = parity;
			MyPort.DataBits = dataBits;
			MyPort.StopBits = stopBits;
		}
		Console.WriteLine ("Settings synchronizated");
		
	}
	protected virtual void OntogglebuttonPortSwitchClicked (object sender, System.EventArgs e)
	{
		// 该函数为串口开关
		Console.WriteLine ("Port switch active");
		if (!MyPort.IsOpen) {
			SettingsSynchronization ();
			MyPort.Open ();
			togglebuttonPortSwitch.Label = "关闭串口(_C)";
			labelPortState.Text = "串口开";
			
		} else {
			if (checkbuttonAutoSend.Active) {
				checkbuttonAutoSend.Active = false;
				OnCheckbuttonAutoSendClicked (this, null);
			}
			MyPort.Close ();
			togglebuttonPortSwitch.Label = "打开串口(_O)";
			labelPortState.Text = "串口关";
			togglebuttonPortSwitch.Active = false;
		}
	}

	protected virtual void OnButtonSendClicked (object sender, System.EventArgs e)
	{
		if (!MyPort.IsOpen) {
			togglebuttonPortSwitch.Active = true;
			if (!MyPort.IsOpen)
				OntogglebuttonPortSwitchClicked (this, null);
		}
		if (MyPort.IsOpen) {
			string strSend = textviewSend.Buffer.Text;
			byte[] sendByte;
			switch (SendMode) {
			case ConvertMode.Text:
				sendByte = new byte[StringConverts.StringToBytes (strSend).Length];
				sendByte = StringConverts.StringToBytes (strSend);
				break;
			case ConvertMode.Hex:
				sendByte = new byte[StringConverts.HexStringToBytes (strSend).Length];
				sendByte = StringConverts.HexStringToBytes (strSend);
				break;
			case ConvertMode.Dec:
				sendByte = new byte[StringConverts.DecStringToBytes (strSend).Length];
				sendByte = StringConverts.DecStringToBytes (strSend);
				break;
			default:
				sendByte = new byte[StringConverts.StringToBytes (strSend).Length];
				sendByte = StringConverts.StringToBytes (strSend);
				break;
			}
			MyPort.Write (sendByte, 0, sendByte.Length);
			TextIter iter;
			iter = textviewTextS.Buffer.EndIter;
			textviewTextS.Buffer.Insert (ref iter, StringConverts.BytesToString (sendByte));
			if (checkbuttonAutoScrollSend.Active) {
				textviewTextS.Buffer.CreateMark ("EndMark", iter, false);
				textviewTextS.ScrollToMark (textviewTextS.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
				textviewTextS.Buffer.DeleteMark ("EndMark");
			}
			iter = textviewHexS.Buffer.EndIter;
			textviewHexS.Buffer.Insert (ref iter, StringConverts.BytesToHexString (sendByte));
			if (checkbuttonAutoScrollSend.Active) {
				textviewHexS.Buffer.CreateMark ("EndMark", iter, false);
				textviewHexS.ScrollToMark (textviewHexS.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
				textviewHexS.Buffer.DeleteMark ("EndMark");
			}
			iter = textviewDecS.Buffer.EndIter;
			textviewDecS.Buffer.Insert (ref iter, StringConverts.BytesToDecString (sendByte));
			if (checkbuttonAutoScrollSend.Active) {
				textviewDecS.Buffer.CreateMark ("EndMark", iter, false);
				textviewDecS.ScrollToMark (textviewDecS.Buffer.CreateMark ("EndMark", iter, false), 0, false, 0, 0);
				textviewDecS.Buffer.DeleteMark ("EndMark");
			}
			
		}
	}

	protected virtual void OnRadiobuttonSendActivated (object sender, System.EventArgs e)
	{
		
		if (radiobuttonText.Active) {
			NowSendMode = ConvertMode.Text;
		} else if (radiobuttonHex.Active) {
			NowSendMode = ConvertMode.Hex;
		} else if (radiobuttonDec.Active) {
			NowSendMode = ConvertMode.Dec;
		}
		if (NowSendMode != SendMode) {
			string strSend = textviewSend.Buffer.Text;
			byte[] sendByte;
			switch (SendMode) {
			case ConvertMode.Text:
				sendByte = new byte[StringConverts.StringToBytes (strSend).Length];
				sendByte = StringConverts.StringToBytes (strSend);
				break;
			case ConvertMode.Hex:
				sendByte = new byte[StringConverts.HexStringToBytes (strSend).Length];
				sendByte = StringConverts.HexStringToBytes (strSend);
				break;
			case ConvertMode.Dec:
				sendByte = new byte[StringConverts.DecStringToBytes (strSend).Length];
				sendByte = StringConverts.DecStringToBytes (strSend);
				break;
			default:
				sendByte = new byte[StringConverts.StringToBytes (strSend).Length];
				sendByte = StringConverts.StringToBytes (strSend);
				break;
			}
			switch (NowSendMode) {
			case ConvertMode.Text:
				strSend = StringConverts.BytesToString (sendByte);
				break;
			case ConvertMode.Hex:
				strSend = StringConverts.BytesToHexString (sendByte);
				break;
			case ConvertMode.Dec:
				strSend = StringConverts.BytesToDecString (sendByte);
				break;
			}
			textviewSend.Buffer.Text = strSend;
			SendMode = NowSendMode;
		}
	}

	protected virtual void OnSpinbuttonIntervalValueChanged (object sender, System.EventArgs e)
	{
		SendTimer.Interval = Convert.ToDouble (spinbuttonInterval.Text);
	}

	protected virtual void OnCheckbuttonAutoSendClicked (object sender, System.EventArgs e)
	{
		if (checkbuttonAutoSend.Active) {
			SendTimer.Enabled = true;
		} else {
			SendTimer.Enabled = false;
		}
	}

	protected virtual void OnButtonClearReceiveAreaClicked (object sender, System.EventArgs e)
	{
		textviewText.Buffer.Clear ();
		textviewHex.Buffer.Clear ();
		textviewDec.Buffer.Clear ();
	}

	protected virtual void OnButtonClearSendAreaClicked (object sender, System.EventArgs e)
	{
		textviewTextS.Buffer.Clear ();
		textviewHexS.Buffer.Clear ();
		textviewDecS.Buffer.Clear ();
	}

	protected virtual void OnButtonClearAllClicked (object sender, System.EventArgs e)
	{
		OnButtonClearReceiveAreaClicked (this, null);
		OnButtonClearSendAreaClicked (this, null);
		
	}

	protected virtual void OnTextviewSendBackspace (object sender, System.EventArgs e)
	{
		if (NowSendMode != ConvertMode.Text) {
			textviewSend.Buffer.Changed -= HandleTextviewSendBufferChanged;
			textviewSend.Buffer.Text = textviewSend.Buffer.Text.TrimEnd (SplitString.ToCharArray ());
			textviewSend.Buffer.Changed += HandleTextviewSendBufferChanged;
		}
	}

	protected virtual void OnQuitActionActivated (object sender, System.EventArgs e)
	{
		OnDeleteEvent (this, null);
	}

	protected virtual void OnAboutActionActivated (object sender, System.EventArgs e)
	{
		if (aboutWindow == null) {
			aboutWindow = new CommBug.AboutWindow ();
		} else {
			if (!aboutWindow.Visible)
				aboutWindow = new CommBug.AboutWindow ();
		}
	}
	
	
	
	
}
