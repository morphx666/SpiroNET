Public Class FormMain
    Private Const ToRad As Double = Math.PI / 180.0
    Private Const ToDeg As Double = 180.0 / Math.PI

    Private Enum GridStyles
        Rectangular
        Circular
    End Enum

    Private divisions As Integer = 24
    Private radius As Integer
    Private center As PointF
    Private gridStyle As GridStyles = GridStyles.Circular
    Private gridMargin As Integer = 10

    Private isLeftMouseDown As Boolean

    Private gridLines As New List(Of PointF)
    Private lines As New List(Of Line)
    Private currentLine As Line

    Private gridPen As New Pen(Color.Gray, 1)

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        AddHandler Me.SizeChanged, Sub() CreateBitmaps()
        AddHandler Me.MouseDown, Sub(s1 As Object, e1 As MouseEventArgs)
                                     If e1.Button = MouseButtons.Left Then
                                         isLeftMouseDown = True
                                         currentLine = New Line(Color.Blue, 2)
                                     End If
                                 End Sub
        AddHandler Me.MouseUp, Sub(s1 As Object, e1 As MouseEventArgs)
                                   If isLeftMouseDown AndAlso e1.Button = MouseButtons.Left Then
                                       isLeftMouseDown = False
                                       lines.Add(currentLine)
                                   End If
                               End Sub

        CreateBitmaps()
    End Sub

    Private Sub CreateBitmaps()
        Init()
    End Sub

    Private Sub Init()
        Dim newCenter As PointF = New PointF(Me.DisplayRectangle.Width / 2, Me.DisplayRectangle.Height / 2)
        Dim newRadius As Integer

        Select Case gridStyle
            Case GridStyles.Rectangular
                newRadius = Math.Min(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height) - gridMargin * 2
                newRadius -= newRadius Mod divisions
            Case GridStyles.Circular
                newRadius = Math.Min(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height) / 2 - gridMargin
        End Select

        If newRadius <= 0 Then Return

        ' Scale points
        If lines.Count > 0 AndAlso newCenter <> center Then
            Dim dx As Double
            Dim dy As Double
            Dim pa As Double
            Dim scale As Double = newRadius / radius
            Dim pr As Double

            For Each l In lines
                For p As Integer = 0 To l.Points.Count - 1
                    dx = l.Points(p).X - center.X
                    dy = l.Points(p).Y - center.Y
                    pa = Math.Atan2(dy, dx)
                    pr = Math.Sqrt(dx ^ 2 + dy ^ 2) * scale
                    l.Points(p) = New PointF(newCenter.X + pr * Math.Cos(-pa), newCenter.Y + pr * Math.Sin(pa))
                Next
            Next
        End If

        center = newCenter
        radius = newRadius

        lines.Clear()
        gridLines.Clear()
        Select Case gridStyle
            Case GridStyles.Rectangular

            Case GridStyles.Circular
                Dim s As Integer = 360 / divisions
                For a As Integer = 0 To 360 - s Step s
                    gridLines.Add(New PointF(center.X + radius * Math.Cos(-a * ToRad), center.Y + radius * Math.Sin(a * ToRad)))
                Next
        End Select

        radius = newRadius
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        g.Clear(Color.WhiteSmoke)

        Select Case gridStyle
            Case GridStyles.Rectangular
                DrawRectangularGrid(g)

                lines.ForEach(Sub(l) DrawRectangularMirroredLines(g, l))
                If isLeftMouseDown Then DrawRectangularMirroredLines(g, currentLine)

            Case GridStyles.Circular
                DrawCircularGrid(g)

                lines.ForEach(Sub(l) DrawCircularMirroredLines(g, l))
                If isLeftMouseDown Then DrawCircularMirroredLines(g, currentLine)
        End Select
    End Sub

    Private Sub DrawRectangularMirroredLines(g As Graphics, line As Line)
        If line.Points.Count < 2 Then Exit Sub

        Dim l As New Line()
        Dim qs As Integer = radius \ divisions
        For y = 0 To divisions - 1
            For x = 0 To divisions - 1
                For p As Integer = 0 To line.Points.Count - 1
                    l.Points.Add(New Point(line.Points(p).X + x * qs, line.Points(p).Y + y * qs))
                Next
                g.DrawLines(line.Pen, l.Points.ToArray())
                l.Points.Clear()
            Next
        Next
    End Sub

    Private Sub DrawRectangularGrid(g As Graphics)
        Dim s As Integer = radius / divisions
        Dim l As Integer = radius + gridMargin

        For y = gridMargin To radius + gridMargin Step s
            g.DrawLine(gridPen, gridMargin, y, l, y)
        Next

        For x = gridMargin To radius + gridMargin Step s
            g.DrawLine(gridPen, x, gridMargin, x, l)
        Next
    End Sub

    Private Sub DrawCircularGrid(g As Graphics)
        gridLines.ForEach(Sub(gl) g.DrawLine(gridPen, center, gl))
        g.DrawEllipse(gridPen, center.X - radius, center.Y - radius, radius * 2, radius * 2)
    End Sub

    ' http://www.drawerings.com/draw
    Private Sub DrawCircularMirroredLines(g As Graphics, line As Line)
        If line.Points.Count < 2 Then Exit Sub

        Dim l As New Line(line)
        Dim s As Integer = 360 / divisions

        For Each gl In gridLines
            ' Project point across line
            ' http://stackoverflow.com/questions/8954326/how-to-calculate-the-mirror-point-along-a-line
            Dim A As Double = gl.Y - center.Y
            Dim B As Double = -(gl.X - center.X)
            Dim C As Double = -A * center.X - B * center.Y

            Dim M As Double = Math.Sqrt(A ^ 2 + B ^ 2)

            Dim A1 As Double = A / M
            Dim B1 As Double = B / M
            Dim C1 As Double = C / M

            For p As Integer = 0 To l.Points.Count - 1
                Dim D As Double = A1 * l.Points(p).X + B1 * l.Points(p).Y + C1
                l.Points(p) = New Point(l.Points(p).X - 2 * A1 * D, l.Points(p).Y - 2 * B1 * D)
            Next

            g.DrawLines(line.Pen, l.Points.ToArray())
        Next

        'For a As Integer = 0 To 360 - s Step s
        '    g.DrawString(a, Me.Font, Brushes.DarkViolet, New Point(center.X + radius * Math.Cos(-a * ToRad), center.Y + radius * Math.Sin(a * ToRad)))
        'Next
    End Sub

    Private Function IsPointValid(p As Point) As Boolean
        Select Case gridStyle
            Case GridStyles.Rectangular : Return (p.X >= gridMargin AndAlso
                                                  p.X < (radius + gridMargin)) AndAlso
                                                  (p.Y >= gridMargin AndAlso
                                                  p.Y < (radius + gridMargin))
            Case GridStyles.Circular : Return (p.X - center.X) ^ 2 / radius ^ 2 +
                                              (p.Y - center.Y) ^ 2 / radius ^ 2 <= 1.0
            Case Else : Return False
        End Select
    End Function

    Private Sub FormMain_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If isLeftMouseDown Then
            If IsPointValid(e.Location) Then
                Select Case gridStyle
                    Case GridStyles.Rectangular
                        Dim qs As Integer = radius \ divisions
                        Dim q As New Point((e.Location.X - gridMargin) \ qs, (e.Location.Y - gridMargin) \ qs)
                        currentLine.Points.Add(New Point(e.Location.X - (q.X * qs), e.Location.Y - (q.Y * qs)))

                    Case GridStyles.Circular
                        currentLine.Points.Add(e.Location)
                End Select

                Me.Invalidate()
            End If
        End If
    End Sub

    Private Sub FormMain_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Tab Then
            If gridStyle = GridStyles.Rectangular Then
                gridStyle = GridStyles.Circular
            Else
                gridStyle = GridStyles.Rectangular
            End If

            Init()
            Me.Invalidate()
        End If
    End Sub
End Class