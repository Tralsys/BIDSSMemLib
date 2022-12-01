// elapseScript.csx
// 1フレームごとに実行されるスクリプト
// Copyright 2022 Tetsu Otter
// License : The MIT License

// 定数定義ファイルを読み込みます
#load "constants.csx"

class SampleDataClass
{
	public bool Is_AtsS_White_Lamp_On;
	public bool Is_AtsS_Red_Lamp_On;

	public bool Is_AtsP_Power_Lamp_On;
	public bool Is_AtsP_Pattern_Approaching_Lamp_On;
	public bool Is_AtsP_Brake_Cut_Off_Lamp_On;
	public bool Is_AtsP_Brake_Working_Lamp_On;
	public bool Is_AtsP_Enabled_Lamp_On;
}

SampleDataClass data = new();

data.Is_AtsS_White_Lamp_On = Panel[0] == 1;
data.Is_AtsS_Red_Lamp_On = Panel[1] == 1;

data.Is_AtsP_Power_Lamp_On = Panel[2] == 1;
data.Is_AtsP_Pattern_Approaching_Lamp_On = Panel[3] == 1;
data.Is_AtsP_Brake_Cut_Off_Lamp_On = Panel[4] == 1;
data.Is_AtsP_Brake_Working_Lamp_On = Panel[5] == 1;
data.Is_AtsP_Enabled_Lamp_On = Panel[6] == 1;

SetValue(ObjectHolder, nameof(SampleDataClass), data);
