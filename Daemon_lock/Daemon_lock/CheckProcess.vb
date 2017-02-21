Public Class CheckProcess
    Public Function confirm(ByVal process As String) As Boolean
        If System.Diagnostics.Process.GetProcessesByName(process).Length > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
