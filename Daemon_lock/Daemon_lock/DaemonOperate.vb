
Public Class DaemonOperate
    Private Declare Function SetForegroundWindow Lib "user32" (ByVal hwnd As IntPtr) As IntPtr
    Private Declare Function PostMessage Lib "user32" Alias "PostMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String) As Integer
    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String) As Integer
    Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" (ByVal hWnd1 As Integer, ByVal hWnd2 As Integer, ByVal lpsz1 As String, ByVal lpsz2 As String) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
    Private Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hwnd As Integer, ByVal nIndex As Integer) As Integer
    Dim pass_word_show = password() & "e"
    Dim pass_word_eng = "Teradyne"
    Dim delay_time = 500
    Dim delay = New delay(100)
    Dim WS_MINIMIZE As Integer = 536870912
    Dim WS_VISIBLE As Integer = 268435456
    Public ReadOnly Property password() As String
        Get
            Return My.Settings.password
        End Get
    End Property

    Function check_hdwn(mode) As String()
        'check daemon is running
        Dim process As New CheckProcess
        If (process.confirm("TesterSvcDaemon")) Then
        Else
            Return New String() {"0", "No  Daemon Start"}
        End If

        Dim hdWn_daemon = 0, hdwn_frame = 0, timeout = 0
        'find daemon hdwn
        Do While (hdWn_daemon = 0 Or hdwn_frame = 0)
            hdWn_daemon = FindWindow("ThunderRT6FormDC", "IG-XL Tester Service Daemon")
            hdwn_frame = FindWindowEx(hdWn_daemon, 0, "ThunderRT6Frame", vbNullString)
            delay.delay()
            timeout += 1
            If timeout > delay_time Then
                Return New String() {"0", "No  Daemon Found"}
            End If
        Loop
        timeout = 0
        Select Case mode
            Case "daemon"
                Return New String() {"1", hdWn_daemon}
                'return mini windows hdwn
            Case "TstSvD"
                Dim hdWn_window = 0
                'find the TstSvc hdwn
                Do While (hdWn_window = 0)
                    hdWn_window = FindWindow("ThunderRT6FormDC", "Open TstSvcD Window")
                    delay.delay()
                    timeout += 1
                    If timeout > delay_time Then
                        Return New String() {"0", "Can't find TstSvcD Window"}
                    End If
                Loop
                Return New String() {"1", hdWn_window}
                'return Engineering button hdwn
            Case "mode"
                Dim hdwn_eng = 0, hdwn_pro = 0
                'find the button hdwn
                Do While (1)
                    hdwn_eng = FindWindowEx(hdwn_frame, 0, "ThunderRT6CommandButton", "Engineering")
                    hdwn_pro = FindWindowEx(hdwn_frame, 0, "ThunderRT6CommandButton", "Production")
                    If (hdwn_eng <> 0) Then
                        Return New String() {"pro", hdwn_eng}
                    End If
                    If (hdwn_pro <> 0) Then
                        Return New String() {"eng", hdwn_pro}
                    End If
                    timeout += 1
                    If timeout > delay_time Then
                        Return New String() {"0", "Error check Mode"}
                    End If
                    delay.delay()
                Loop
            Case "unload"
                Dim hdwn_frame1 = 0, hdwn_unload = 0, hdwn_load = 0, hdwn_frame2 = 0
                'find the unload frame
                Do While (hdwn_frame1 = 0 Or hdwn_frame2 = 0)
                    hdwn_frame1 = FindWindowEx(hdWn_daemon, 0, "ThunderRT6Frame", vbNullString)
                    hdwn_frame2 = FindWindowEx(hdwn_frame1, 0, "ThunderRT6Frame", "Load Options")
                    delay.delay()
                    timeout += 1
                    If timeout > delay_time Then
                        Return New String() {"0", "Pls Check Daemon Version"}
                    End If
                Loop
                timeout = 0
                'Findthe unload button
                Do While (1)
                    hdwn_unload = FindWindowEx(hdwn_frame2, 0, "ThunderRT6CommandButton", "Unload")
                    hdwn_load = FindWindowEx(hdwn_frame2, 0, "ThunderRT6CommandButton", "Test Program Workbook")
                    If hdwn_unload <> 0 Then
                        Return New String() {"1", hdwn_unload}
                    End If
                    If hdwn_load <> 0 Then
                        Return New String() {"2", hdwn_load}
                    End If
                    delay.delay()
                    timeout += 1
                    If timeout > delay_time Then
                        Return New String() {"0", "Unload Error"}
                    End If
                Loop

        End Select
        Return New String() {"0", "Error Command"}
    End Function
    Private Function unload() As Boolean
        Dim unloadjob() As String = check_hdwn("unload")
        If unloadjob(0) = "1" Then
            SetForegroundWindow(unloadjob(1))
            PostMessage(unloadjob(1), 245, 0, 0)
        End If
        Dim check_unload(2) As String
        Dim timeout = 0
        Do While (check_unload(0) = "1")
            check_unload = check_hdwn("unload")
            delay.delay()
            timeout += 1
            If timeout > delay_time Then
                Return False
            End If
        Loop
        Return True
    End Function
    Function daemon_recovery() As Boolean
        Dim check_daemon(2) As String
        'get daemon hdwn
        check_daemon = check_hdwn("daemon")
        If check_daemon(0) = 0 Then
            Return False
        End If
        Dim window_style = GetWindowLong(check_daemon(1), -16)
        Dim minisize = window_style And WS_MINIMIZE
        Dim visable = window_style And WS_VISIBLE
        If ((minisize <> 0 And visable <> 0) Or visable = 0) Then
            PostMessage(check_daemon(1), 274, 61728, 0)
            Dim recovery_daemon(2) As String
            'get the TstSvD hdwn
            recovery_daemon = check_hdwn("TstSvD")
            If recovery_daemon(0) = "1" Then
                'recovery daemon
                Dim hdwn_edit = 0, hdwn_ok = 0, timeout = 0
                Do While (hdwn_edit = 0 Or hdwn_ok = 0)
                    hdwn_edit = FindWindowEx(recovery_daemon(1), 0, "ThunderRT6TextBox", vbNullString)
                    hdwn_ok = FindWindowEx(recovery_daemon(1), 0, "ThunderRT6CommandButton", "ok")
                    delay.delay()
                    timeout += 1
                    If timeout > delay_time Then
                        Return False
                    End If
                Loop
                SendMessage(hdwn_edit, 12, 0, pass_word_show)
                SendMessage(hdwn_ok, 245, 0, 0)
                Return True
            Else
                'can't find  TstSvD hdwn
                Return False
            End If
        Else
            Return True
        End If
    End Function
    Function daemon_eng() As String()
        Dim check_eng(2) As String
        'return eng button
        check_eng = check_hdwn("mode")
        If check_eng(0) = "pro" Then
            Dim show As Boolean = daemon_recovery()
            If (show) Then
                Dim Eng_hdwn As Integer = check_eng(1)
                SetForegroundWindow(Eng_hdwn)
                PostMessage(Eng_hdwn, 245, 0, 0)
                'input pass word
                Dim hdWn_engmode = 0, hdwn_edit = 0, hdwn_ok = 0, timeout = 0
                Do While (hdWn_engmode = 0)
                    hdWn_engmode = FindWindow("ThunderRT6FormDC", "Engineering Mode")
                    If (hdWn_engmode = 0) Then
                        SetForegroundWindow(Eng_hdwn)
                        PostMessage(Eng_hdwn, 245, 0, 0)
                    End If
                    If timeout > delay_time Then
                        'mini size daemon
                        Dim daemon_hdwn = check_hdwn("daemon")
                        PostMessage(daemon_hdwn(1), 274, 61472, 0)
                        Return {0, "Entry Engineer Mode Error"}
                    End If
                    delay.delay()
                    timeout += 1
                Loop
                timeout = 0
                Do While (hdwn_ok = 0 Or hdwn_edit = 0)
                    hdwn_edit = FindWindowEx(hdWn_engmode, 0, "ThunderRT6TextBox", vbNullString)
                    hdwn_ok = FindWindowEx(hdWn_engmode, 0, "ThunderRT6CommandButton", "OK")
                    If timeout > delay_time Then
                        Return {0, "Entry Engineer Mode Error"}
                    End If
                Loop
                SendMessage(hdWn_engmode, 274, 61728, 0)
                SendMessage(hdwn_edit, 12, 0, pass_word_eng)
                'set Focus on "Engineer" button
                Dim window_style = 1
                timeout = 0
                'window get visible styele
                Do While (window_style And WS_VISIBLE <> 0)
                    window_style = GetWindowLong(hdWn_engmode, -16)
                    SetForegroundWindow(hdwn_ok)
                    PostMessage(hdwn_ok, 245, 0, 0)
                    If timeout > delay_time Then
                        Return {0, "Entry Engineer Mode Error"}
                    End If
                    delay.delay()
                    timeout += 1
                Loop
            Else
                Return {0, "Recovery Daemon Error"}
            End If
        ElseIf check_eng(0) = "eng" Then
            'already Eng mode
        Else : Return check_eng
        End If
        Dim checkpro = check_hdwn("mode")
        If checkpro(0) = "eng" Then
            Return {1, "Change Engineer Mode done"}
        Else
            'mini size daemon
            Dim daemon_hdwn = check_hdwn("daemon")
            PostMessage(daemon_hdwn(1), 274, 61472, 0)
            Return {0, "Change Engineer Mode Error"}
        End If
    End Function
    Function daemon_pro() As String()
        'close config windows to avoid minsize daemon error
        Dim daemon_config_hdwn = FindWindow("ThunderRT6FormDC", "IG-XL Tester Service Daemon Configuration")
        If daemon_config_hdwn <> 0 Then
            PostMessage(daemon_config_hdwn, 16, 0, 0)
        End If
        'unload job
        Dim unloadjob As Boolean = unload()
        If (unloadjob) Then
            Dim check_pro As String() = check_hdwn("mode")
            If check_pro(0) = "eng" Then
                Dim show As Boolean = daemon_recovery()
                If (show) Then
                    Dim Pro_hdwn As Integer = check_pro(1)
                    SetForegroundWindow(Pro_hdwn)
                    SendMessage(Pro_hdwn, 245, 0, 0)
                Else
                    Return {0, "Recovery Daemon Error"}
                End If
            ElseIf check_pro(0) = "pro" Then
                'already Pro mode
            Else : Return {0, check_pro(1)}
            End If
            Dim checkeng = check_hdwn("mode")
            If checkeng(0) = "pro" Then
                Dim daemon_hdwn = check_hdwn("daemon")
                PostMessage(daemon_hdwn(1), 274, 61472, 0)
                Return {1, "Change Pro Mode done"}
            Else
                Return {0, "Change Pro Mode Error"}
            End If
        Else
            Return {0, "Unload Program Error"}
        End If
    End Function
End Class
