Imports System
Imports System.IO
Imports System.IO.Ports
Imports System.ComponentModel
Imports System.Threading
Imports System.Runtime.InteropServices
Public Class Form1
    Dim myPort As Array
    Delegate Sub SetTextCallback(ByVal [text] As String)


    Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
        SerialPort1.Write(btnSend.Text & vbCr)
    End Sub

    Private Sub btnConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConnect.Click
        SerialPort1.PortName = cmbPort.Text
        SerialPort1.BaudRate = cmbBaud.Text
        SerialPort1.Parity = IO.Ports.Parity.None
        SerialPort1.StopBits = IO.Ports.StopBits.One
        SerialPort1.DataBits = 8
        SerialPort1.Open()
        btnConnect.Enabled = False
        btnDisconnect.Enabled = True
    End Sub
    Public Sub SerialPort1_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SerialPort1.DataReceived
        Dim s As String = SerialPort1.ReadExisting()
        ReceivedText(s)
        Dim Writer As System.IO.StreamWriter
        'Dim s1 As String = s.Remove(s.Length - 1)
        Dim s1 = Microsoft.VisualBasic.Right(s, 1)
        Dim s2 As String
        s2 = s.Remove(s.Length - 2)
        Dim x2 As Double
        x2 = CDbl(s2)
        Dim Y As Database1DataSet.SensorLogRow
        Y = Database1DataSet.SensorLog.NewRow
        Y._Value_Reading = x2
        Y.Sensor = s1
        Y.DateTime = System.DateTime.Now
        Database1DataSet.SensorLog.Rows.Add(Y)
        Me.TableAdapterManager.UpdateAll(Me.Database1DataSet)
        Select Case s1
            Case "C"
                Dim strPath As String = "D:\Tempt.csv"
                If File.Exists(strPath) = False Then
                    Writer = File.CreateText(strPath)
                    Writer.Close()
                End If
                Writer = File.AppendText(strPath)
                Writer.Write(s)
                Writer.Write(",")
                Writer.Flush()
                Writer.Close()
                Me.Chart2.Series(0).Points.AddY(x2)
                System.Threading.Thread.Sleep(200)

            Case "V"
                Dim strPath As String = "D:\Light.csv"
                If File.Exists(strPath) = False Then
                    Writer = File.CreateText(strPath)
                    Writer.Close()
                End If
                Writer = File.AppendText(strPath)
                Writer.Write(s)
                Writer.Write(",")
                Writer.Flush()
                Writer.Close()
                Me.Chart1.Series(0).Points.AddY(x2)
                System.Threading.Thread.Sleep(200)

            Case Else

        End Select
        Me.Validate()
        Me.SensorLogBindingSource.EndEdit()
        Me.TableAdapterManager.UpdateAll(Me.Database1DataSet)
        s2 = ""
        s = ""
        SerialPort1.DiscardInBuffer()
    End Sub

    Private Sub ReceivedText(ByVal [text] As String)
        If Me.rtbReceived.InvokeRequired Then
            Dim x As New SetTextCallback(AddressOf ReceivedText)
            Me.Invoke(x, New Object() {(text)})
        Else
            Me.rtbReceived.Text &= [text]
        End If

    End Sub

    Private Sub btnDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDisconnect.Click
        SerialPort1.DiscardInBuffer()
        SerialPort1.Close()
        btnConnect.Enabled = True
        btnDisconnect.Enabled = False
    End Sub
    Private Sub cmbBaud_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbBaud.SelectedIndexChanged
        If SerialPort1.IsOpen = False Then
            SerialPort1.BaudRate = cmbBaud.Text
        Else
            MsgBox("Valid only if closed", vbCritical)
        End If
    End Sub

    Private Sub cmbPort_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPort.SelectedIndexChanged
        If SerialPort1.IsOpen = False Then
            SerialPort1.PortName = cmbPort.Text
        Else
            MsgBox("Valid only if closed", vbCritical)
        End If
    End Sub
    Public Sub Chart2_Load()
        Using Reader As New Microsoft.VisualBasic.FileIO.TextFieldParser("D:\Tempt.csv")
            Reader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            Reader.Delimiters = New String() {"C,"}
            Dim currentRow As String()
            Dim CurrentPoint As Integer = 0
            Me.Chart2.Series(0).Points.Clear()
            While Not Reader.EndOfData
                Try
                    currentRow = Reader.ReadFields()
                    Dim currentfield As String
                    For Each currentfield In currentRow
                        Me.Chart2.Series(0).Points.AddY(currentfield)
                    Next

                Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                    If CurrentPoint <> 0 Then
                        MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                    End If
                End Try
            End While
        End Using
    End Sub

    Public Sub Chart1_Load()
        Using Reader As New Microsoft.VisualBasic.FileIO.TextFieldParser("D:\Light.csv")
            Reader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            Reader.Delimiters = New String() {"V,"}
            Dim currentRow As String()
            Dim CurrentPoint As Integer = 0
            Me.Chart1.Series(0).Points.Clear()
            While Not Reader.EndOfData
                Try
                    currentRow = Reader.ReadFields()
                    Dim currentfield As String
                    For Each currentfield In currentRow
                        Me.Chart1.Series(0).Points.AddY(currentfield)
                    Next

                Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                    If CurrentPoint <> 0 Then
                        MsgBox("Line " & ex.Message & " is invalid.  Skipping")
                    End If
                End Try
            End While
        End Using
    End Sub
    Private Sub SensorLogBindingNavigatorSaveItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SensorLogBindingNavigatorSaveItem.Click
        Me.Validate()
        Me.SensorLogBindingSource.EndEdit()
        Me.TableAdapterManager.UpdateAll(Me.Database1DataSet)
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.SensorLogTableAdapter.Fill(Me.Database1DataSet.SensorLog)
        myPort = IO.Ports.SerialPort.GetPortNames
        cmbBaud.Items.Add(2400)
        cmbBaud.Items.Add(4800)
        cmbBaud.Items.Add(9600)
        cmbBaud.Items.Add(19200)
        For i = 0 To UBound(myPort)
            cmbPort.Items.Add(myPort(i))
        Next
        cmbPort.Text = cmbPort.Items.Item(0)
        cmbBaud.Text = cmbBaud.Items.Item(0)
        btnDisconnect.Enabled = False
        cmbSesl.Items.Add("Temperature")
        cmbSesl.Items.Add("Light")
        Dim Writer As System.IO.StreamWriter
        Dim strPath As String = "D:\Tempt.csv"
        If File.Exists(strPath) = False Then
            Writer = File.CreateText(strPath)
            Writer.Close()
        End If
        strPath = "D:\Light.csv"
        If File.Exists(strPath) = False Then
            Writer = File.CreateText(strPath)
            Writer.Close()
        End If
        cmbSesl.Text = cmbSesl.Items.Item(0)
        Chart1_Load()
        Chart2_Load()
    End Sub

    Private Sub cmbSesl_SelectedIndexChanged_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbSesl.SelectedIndexChanged
        If cmbSesl.Text = "Light" Then
            Chart2.Hide()
            Chart1.Show()
            Chart1_Load()
        Else
            Chart1.Hide()
            Chart2.Show()
            Chart2_Load()
        End If
    End Sub

End Class
