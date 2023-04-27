Imports PoohPlcLink
Imports FireSharp.Config
Imports FireSharp.Response
Imports FireSharp.Interfaces
Public Class Form1

    Public PLC As New PoohFinsETN


    Dim S1 As Integer = 0
    Dim S2 As Integer = 0

    Dim S3 As Integer = 0
    Dim S4 As Integer = 0

    Dim S5 As Integer = 0
    Dim S6 As Integer = 0

    Dim Wp As Integer = 0
    Dim Mp As Integer = 0

    Dim A1 As Integer = 0
    Dim A2 As Integer = 0

    Dim Value_w(0) As Integer 'จำนวนชิ้นงานประเภทไม้ Work area 8 (W8)
    Dim Value_m(0) As Integer 'จำนวนชิ้นงานประเภทโลหะ Work area 7 (W7)

    Dim Counter As Integer = 0

    Dim WC As Integer = 0
    Dim MC As Integer = 0

    'Setting firebase

    Private fcon As New FirebaseConfig() With
        {
        .AuthSecret = "ElUTnONiy3BZ6JG13KyHGtC1maDPnG4OQOQZ0hsr",
        .BasePath = "https://projectplc-34792-default-rtdb.firebaseio.com/"
        }

    Private client As IFirebaseClient

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            client = New FireSharp.FirebaseClient(fcon)
        Catch ex As Exception
            MessageBox.Show("There was a problem in the internet connection")
        End Try

        Timer1.Interval = 10
        Timer1.Start()

        '***** ติดต่อกับ PLC *****'
        With PLC
            .PC_NetNo = Val(0)
            .PC_NodeNo = Val(1) 'จะกำหนดเป็น 192.168.250.1
            .PLC_IPAddress = "192.168.250.144"
            .PLC_UDPPort = Val(9600)
            .PLC_NetNo = Val(0)
            .PLC_NodeNo = Val(144)
        End With

        PLC.WriteMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(23), 0, PoohFinsETN.DataTypes.SignBIN)

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        'ใช้ติดต่อกับ PLC แค่นี้เอง
        Value_w = PLC.ReadMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(8), Val(1), PoohFinsETN.DataTypes.SignBIN)
        Value_m = PLC.ReadMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(7), Val(1), PoohFinsETN.DataTypes.SignBIN)
        S1 = Value_w(0)
        S3 = Value_m(0)

        txtnumwood.Text = S1.ToString
        txtnummetal.Text = S3.ToString
        txtnumtotal.Text = (S1 + S3).ToString
        txtDate.Text = Date.Now.ToString("dd MMM yyyy")
        txtTime.Text = Date.Now.ToString("HH:mm:ss")

        ShowResult()
        UpResult()

    End Sub

    Private Sub ShowResult()

        If S1 <> S2 Or S3 <> S4 Then

            S2 = S1
            S4 = S3

            A1 = A1 + 1

        End If

    End Sub

    Private Sub UpResult()

        If S2 + S4 <> 0 And A1 <> A2 Then

            DataGridView1.Rows.Add(A1, S1, S3, S1 + S3, Date.Now.ToString("HH:mm:ss"), Date.Now.ToString("dd MMM yyyy"))
            Showinfire()

        End If

        A2 = A1

    End Sub

    'To show infomation in firebase
    Private Sub Showinfire()
        Wp = S1
        Mp = S3
        Dim info As New Workpiece() With
            {
            .Woodpiece = Wp.ToString,
            .Metalpiece = Mp.ToString,
            .ZTotal = (S1 + S3).ToString,
            .Dateday = txtDate.Text
         }

        'Counter'
        'Dim res = client.Get("Counter")
        'Dim Counter = (Integer.Parse(res.ResultAs(Of String)) + 1)
        Counter = Counter + 1
        Dim set2 = client.Set("Counter", Counter)


        'Database'

        Dim setter = client.Set("Database/" + txtTime.Text, info)

        'number of each type of workpiece
        'Dim WCP = client.Get("Total Woodpiece")
        'Dim MCP = client.Get("Total Metalpiece")

        If S1 <> S5 Then
            'Dim WC = (Integer.Parse(WCP.ResultAs(Of String)) + 1)
            WC = WC + 1
            Dim set3 = client.Set("Total Woodpiece", WC)
            S5 = S1

        ElseIf S3 <> S6 Then
            'Dim MC = (Integer.Parse(MCP.ResultAs(Of String)) + 1)
            MC = MC + 1
            Dim set4 = client.Set("Total Metalpiece", MC)
            S6 = S3
        End If

    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click


        'Delete data in firebase'
        Dim res = client.Delete("Database/")
        Dim set2 = client.Set("Counter", 0)
        Dim set3 = client.Set("Total Woodpiece", 0)
        Dim set4 = client.Set("Total Metalpiece", 0)

        'Delete data in datagridview
        DataGridView1.Rows.Clear()

        'For Each Row As DataGridViewRow In DataGridView1.SelectedRows
        '    DataGridView1.Rows.Remove(Row)
        'Next

        S1 = 0
        S2 = 0
        S3 = 0
        S4 = 0
        S5 = 0
        S6 = 0

        A1 = 0
        A2 = 0

        Counter = 0
        WC = 0
        MC = 0

    End Sub

    Private Sub btnEnter_Click(sender As Object, e As EventArgs) Handles btnEnter.Click

        'Write data to PLC
        Dim i As Integer
        i = Int(txtwnum.Text)

        'Which Workarea
        PLC.WriteMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(23), i, PoohFinsETN.DataTypes.SignBIN)

        'Read that Workarea
        Dim Read(0) As Integer
        Read = PLC.ReadMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(23), Val(1), PoohFinsETN.DataTypes.SignBIN)
        txtwval.Text = Read(0).ToString

    End Sub

    'input from firebase
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnfirebase.Click

        Dim res = client.Get("Input_From_Firebase/")

        Dim inputfire = res.ResultAs(Of Integer)

        'txtwnum.Text = enter.ToString

        PLC.WriteMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(23), inputfire, PoohFinsETN.DataTypes.SignBIN)

        'Read that Workarea
        Dim Readfire(0) As Integer
        Readfire = PLC.ReadMemoryWord(PoohFinsETN.MemoryTypes.WR, Val(23), Val(1), PoohFinsETN.DataTypes.SignBIN)
        txtwval.Text = Readfire(0).ToString

    End Sub

End Class
