Imports System.Drawing.Text
Imports MySql.Data.MySqlClient
Public Class Form1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Form2.Show()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Form3.Show()

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Form4.Show()

    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Form5.Show()

    End Sub


    Private connectionString As String = "Server=localhost;Database=project;User Id=root;Password=Tiger123;"

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        ' Show a message box to enter the batch
        Dim batch As String = InputBox("Enter the batch:", "Create Table")

        ' Check if the batch is not empty
        If String.IsNullOrEmpty(batch) Then
            MessageBox.Show("Please enter a batch value.")
            Return
        End If

        ' Check if the table already exists
        Using connection As New MySqlConnection(connectionString)
            connection.Open()
            Using command As New MySqlCommand("SHOW TABLES LIKE @batch", connection)
                command.Parameters.AddWithValue("@batch", batch)
                Dim result As Object = command.ExecuteScalar()
                If result IsNot Nothing Then
                    MessageBox.Show("Table already exists.")
                    Return
                End If
            End Using
        End Using

        ' Create the table
        Using connection As New MySqlConnection(connectionString)
            connection.Open()
            Using command As New MySqlCommand("CREATE TABLE `" & batch & "` (registrationnumber INT PRIMARY KEY, name VARCHAR(50), fname VARCHAR(50), gen VARCHAR(10), catg VARCHAR(10), relg VARCHAR(10), ews ENUM('yes', 'no'), minority ENUM('yes', 'no'))", connection)
                command.ExecuteNonQuery()
                MessageBox.Show("Table created successfully.")
            End Using
        End Using
    End Sub
    Private glowTimer As New Timer()
    Private glowColor As Color = Color.Blue
    Private glowIncrement As Integer = 20
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialize the timer
        glowTimer.Interval = 50
        glowTimer.Enabled = True
        AddHandler glowTimer.Tick, AddressOf GlowTimer_Tick

    End Sub
    Private Sub GlowTimer_Tick(sender As Object, e As EventArgs)
        ' Change the LinkLabel's color
        LinkLabel1.LinkColor = glowColor

        ' Increment the glow color
        Dim r As Integer = glowColor.R + glowIncrement
        Dim g As Integer = glowColor.G + glowIncrement
        Dim b As Integer = glowColor.B + glowIncrement

        ' Ensure the RGB components do not exceed 255
        r = Math.Min(r, 255)
        g = Math.Min(g, 255)
        b = Math.Min(b, 255)

        ' Update the glow color
        glowColor = Color.FromArgb(r, g, b)

        ' Check if the glow color has reached its maximum value
        If r = 255 AndAlso g = 255 AndAlso b = 255 Then
            ' Reset the glow color
            glowColor = Color.Blue
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim response As DialogResult = MessageBox.Show("Are you sure you want to close the application?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If response = DialogResult.Yes Then
            ' Create a list to store the forms to be closed
            Dim formsToClose As New List(Of Form)
            For Each form As Form In Application.OpenForms
                formsToClose.Add(form)
            Next

            ' Close the forms
            For Each form As Form In formsToClose
                form.Close()
            Next

            ' Exit the application
            AddHandler Application.ThreadExit, AddressOf OnThreadExit
            Application.Exit()
        End If
    End Sub
    Private Sub OnThreadExit(sender As Object, e As EventArgs)
        Environment.Exit(0)
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Me.Hide()
        e.Cancel = True
    End Sub
End Class
