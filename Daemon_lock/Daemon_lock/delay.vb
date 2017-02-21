Public Class delay
    Private time As Integer = 100

    Public Sub New(ByVal time As Integer)
        Me.time = time
    End Sub
    Public Sub delay()
        System.Threading.Thread.Sleep(time)
    End Sub
End Class
