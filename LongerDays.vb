Imports GTA
Imports GTA.Math
Imports System
Imports System.IO
Imports System.Text
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports System.Collections.Generic


Public Module GlobalInfo

    Public DebugInfo As String = "Debug Initialized"
    Public DebugMode As Boolean = False

    Public ActiveTimeScale As Single = 0.5
    Public RoundingError As Single = 0
    Public ActualTimePassed As Integer = 0

    Public lastTick As Integer = 0
    Public nowTick As Integer = 0
End Module

Public Class TimeScaler
    Inherits Script

    Private areSettingsLoaded As Boolean = False

    Private TimeScaleDay As Single = 0.5
    Private TimeScaleNight As Single = 0.5

    Private NightStart As Integer = 22
    Private NightEnd As Integer = 7
    Private SkipNight As Boolean = False

    Private lastMin As Integer = 0
    Private nowMin As Integer = 0
    Private lastMinTick As Integer = 0
    Private nowMinTick As Integer = 0

    Const ConversionRate As Single = 0.03

    Public Sub New()
        Me.Interval = 50

        nowTick = Game.GameTime
        lastTick = nowTick

        nowMinTick = Game.GameTime
        lastMinTick = nowMinTick

        PauseTime()
    End Sub

    Public Sub Update(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Tick

        If areSettingsLoaded = False Then
            LoadSettings()
        End If

        Dim h As Integer = World.CurrentDayTime.Hours
        Dim m As Integer = World.CurrentDayTime.Minutes

        lastMin = nowMin
        nowMin = m

        If lastMin <> nowMin Then
            lastMinTick = nowMinTick
            nowMinTick = Game.GameTime
            ActualTimePassed = nowMinTick - lastMinTick
        End If


        If h >= NightStart Or h < NightEnd Then
            ActiveTimeScale = TimeScaleNight
        Else
            ActiveTimeScale = TimeScaleDay
        End If

        lastTick = nowTick
        nowTick = Game.GameTime

        Dim RealTimePassed As Single = nowTick - lastTick
        Dim IngameSecondsPassed As Single = RealTimePassed * ConversionRate * ActiveTimeScale
        Dim Correction As Integer

        If RoundingError <= -1 Then
            Correction = Math.Truncate(RoundingError)
            RoundingError -= Correction
            RoundingError += (IngameSecondsPassed - CInt(IngameSecondsPassed))
            IngameSecondsPassed += Correction
        Else
            RoundingError += (IngameSecondsPassed - CInt(IngameSecondsPassed))
        End If

        DebugInfo = "REQ: " & Math.Round(IngameSecondsPassed, 2) & " /ACT: " & CInt(IngameSecondsPassed) & " /CORR: " & Correction

        AddSecond(CSng(IngameSecondsPassed))

        If SkipNight = True And h >= NightStart Then
            GTA.Native.Function.Call(Native.Hash.SET_CLOCK_TIME, NightEnd, 0, 0)
        End If

    End Sub

    Public Sub LoadSettings()
        Dim val1, val2, val3, val4, val5, val6 As String
        val1 = Settings.GetValue("SETTINGS", "TimeScaleDay", "0.5")
        val2 = Settings.GetValue("SETTINGS", "TimeScaleNight", "0.5")
        val3 = Settings.GetValue("SETTINGS", "NightStart", "22")
        val4 = Settings.GetValue("SETTINGS", "NightEnd", "6")
        val5 = Settings.GetValue("SETTINGS", "Debug", "0")
        val6 = Settings.GetValue("SETTINGS", "SkipNight", "0")

        TimeScaleDay = CSng(val1)
        TimeScaleNight = CSng(val2)
        NightStart = CInt(val3)
        NightEnd = CInt(val4)
        DebugMode = CBool(val5)
        SkipNight = CBool(val6)

        If TimeScaleDay < 0 Then TimeScaleDay = 0
        If TimeScaleNight < 0 Then TimeScaleNight = 0

        areSettingsLoaded = True
    End Sub

    Public Sub PauseTime()
        GTA.Native.Function.Call(Native.Hash.PAUSE_CLOCK, True)
    End Sub

    Public Sub AddSecond(amount As Integer)
        GTA.Native.Function.Call(Native.Hash.ADD_TO_CLOCK_TIME, 0, 0, amount)
    End Sub
End Class

Public Class TimeScalerUI
    Inherits Script

    Private UI_Debug_width As Integer = 180
    Private UI_Debug_height As Integer = 200
    Private UI_Debug As New UIContainer(New Point(1280 - UI_Debug_width, 150), New Size(UI_Debug_width, UI_Debug_height), Color.FromArgb(170, 0, 0, 0)) With {.Enabled = True}

    Public Sub Update(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Tick
        drawUI_Debug()
    End Sub

    Public Sub drawUI_Debug()
        If UI_Debug.Enabled = False Then Exit Sub
        If DebugMode = False Then Exit Sub

        UI_Debug.Items.Clear()
        Dim row As Integer = -1
        Dim txt As String

        row += 1
        txt = "TIMESCALER DEBUG WINDOW"
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        UI_Debug.Items.Add(New UIText(DebugInfo, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Game Time: " & Math.Round(Game.GameTime / 1000, 2)
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Last Tick: " & lastTick
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Time Between Ticks: " & nowTick - lastTick
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Game Clock: " & World.CurrentDayTime.Hours.ToString("D2") & ":" & World.CurrentDayTime.Minutes.ToString("D2") & " (" & ActualTimePassed & "ms)"
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Active Timescale: " & activetimescale
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        row += 1
        txt = "Rounding Error: " & Math.Round(RoundingError, 4)
        UI_Debug.Items.Add(New UIText(txt, New Point(0, 10 * row), 0.2, Color.White, 0, False))

        UI_Debug.Draw()
    End Sub
End Class