Imports System.Net.Sockets
Imports System.Net
Imports System.Text
Imports System.Threading
Public Class Main

    Dim s As Socket = Nothing
    'socket client thread
    Dim t As Thread
    Dim daemon As New DaemonOperate
    'socket return value
    Dim sendStr As String
    Public ReadOnly Property socketport() As String
        Get
            Return My.Settings.port
        End Get
    End Property
    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim check_pro(2) As String
        Control.CheckForIllegalCrossThreadCalls = False
        t = New Thread(AddressOf WaitData)
        t.Start()
        check_pro = daemon.check_hdwn("mode")
        If check_pro(0) = "eng" Then
            Mode.Text = "Eng"
            Engineer.Enabled = False
            Production.Enabled = True
            Recovery.Enabled = True
        ElseIf check_pro(0) = "pro" Then
            Mode.Text = "Pro"
            Engineer.Enabled = True
            Production.Enabled = False
            Recovery.Enabled = False
        Else
            Mode.Text = "Pro"
            Engineer.Enabled = True
            Production.Enabled = False
            Recovery.Enabled = False
            megbox("Pls Open Daemon")
        End If
    End Sub
    Private Sub Main_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        Try
            s.Close()
            t.Abort()
        Catch
        End Try
    End Sub
    Public Sub WaitData()
        Dim local As String
        s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Dim Ip() As System.Net.IPAddress = (System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName))
        For Each Address In Ip '遍历本地的所有可用ip地址。
            If Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
                local = Address.ToString
                If (local Like "10.192*") Then
                    textbox_ip.Text = local
                    Exit For
                End If
            End If
        Next
        Try
            Dim localEndPoint As New IPEndPoint(IPAddress.Parse(local), socketport)
            s.Bind(localEndPoint)
        Catch ex As Exception
            MsgBox(ex.Message)
            Me.Close()
        End Try
        s.Listen(1)
        While (True)
            Dim bytes(4) As Byte
            Dim recive As Socket = s.Accept()
            recive.Receive(bytes)
            Dim mseeage = Encoding.UTF8.GetString(bytes)
            Select Case mseeage
                Case "cheng"
                    Dim result = change_eng_mode()
                    If result = "1" Then
                        sendStr = "Change mode success"
                    End If
                    Dim bs = Encoding.UTF8.GetBytes(sendStr)
                    recive.Send(bs, bs.Length, 0)
                Case "chpro"
                    Dim result = change_pro_mode()
                    If result = "1" Then
                        sendStr = "Change mode success"
                    End If
                    Dim bs = Encoding.UTF8.GetBytes(sendStr)
                    recive.Send(bs, bs.Length, 0)
                Case "check"
                    Dim sendStr = "pro"
                    Dim check_daemon(2) As String
                    check_daemon = daemon.check_hdwn("pro")
                    If check_daemon(0) = "1" Then
                        sendStr = "eng"
                    End If
                    Dim bs = Encoding.UTF8.GetBytes(sendStr)
                    recive.Send(bs, bs.Length, 0)
                Case Else
                    Dim bs = Encoding.UTF8.GetBytes("Error Commnad")
                    recive.Send(bs, bs.Length, 0)
            End Select
            recive.Close()
        End While
    End Sub


    Function change_eng_mode()
        Dim eng As String()
        eng = daemon.daemon_eng()
        If eng(0) = "1" Then
            Production.Enabled = True
            Engineer.Enabled = False
            Recovery.Enabled = True
            Mode.Text = "Eng"
            Return "1"
        Else
            megbox(eng(1))
            Return "0"
        End If
    End Function

    Function change_pro_mode()
        Dim pro As String()
        pro = daemon.daemon_pro()
        If pro(0) = "1" Then
            Production.Enabled = False
            Engineer.Enabled = True
            Recovery.Enabled = False
            Mode.Text = "Pro"
            Return "1"
        Else
            megbox(pro(1))
            Return "0"
        End If

    End Function

    Sub megbox(mesage)
        'check exceed process is exist
        Dim process As New CheckProcess
        Dim flag_exceed As Boolean = process.confirm("exceed")
        If flag_exceed = False Then
            MsgBox(mesage)
        Else
            sendStr = mesage
        End If
    End Sub

    Private Sub Production_Click(sender As Object, e As EventArgs) Handles Production.Click
        'check exceed process is exist
        Dim process As New CheckProcess
        Dim flag_exceed As Boolean = process.confirm("exceed")
        If flag_exceed = True Then
            MsgBox("Pls close Exceed")
        Else
            Dim answer = MsgBox("Do you want to change Pro mode and Unload program?", MsgBoxStyle.YesNo)
            If answer = vbYes Then
                change_pro_mode()
            End If
        End If

    End Sub

    Private Sub Engineer_Click(sender As Object, e As EventArgs) Handles Engineer.Click
        'check exceed process is exist
        Dim process As New CheckProcess
        Dim flag_exceed As Boolean = process.confirm("exceed")
        If flag_exceed = True Then
            MsgBox("Pls close Exceed")
        Else
            change_eng_mode()
        End If
    End Sub

    Private Sub Recovery_Click(sender As Object, e As EventArgs) Handles Recovery.Click
        Dim recovery As Boolean
        recovery = daemon.daemon_recovery()
    End Sub
End Class
