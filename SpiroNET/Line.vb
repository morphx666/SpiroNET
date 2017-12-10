Public Class Line
    Private mColor As Color = Color.Black
    Private mWidth As Single = 1.0
    Private mPoints As New List(Of PointF)

    Private mPen As Pen

    Public Sub New()
        SetupPen()
    End Sub

    Public Sub New(color As Color)
        mColor = color
        SetupPen()
    End Sub

    Public Sub New(color As Color, width As Single)
        mColor = color
        mWidth = width
        SetupPen()
    End Sub

    Public Sub New(line As Line)
        mColor = line.Color
        For Each p As PointF In line.Points
            mPoints.Add(New Point(p.X, p.Y))
        Next
        SetupPen()
    End Sub

    Public ReadOnly Property Color As Color
        Get
            Return mColor
        End Get
    End Property

    Public ReadOnly Property Points As List(Of PointF)
        Get
            Return mPoints
        End Get
    End Property

    Public ReadOnly Property Pen As Pen
        Get
            Return mPen
        End Get
    End Property

    Private Sub SetupPen()
        mPen = New Pen(mColor, mWidth)
    End Sub
End Class