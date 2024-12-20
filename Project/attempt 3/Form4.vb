Imports System.Data.SqlClient
Imports System.Diagnostics.Eventing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Microsoft.SqlServer
Imports MySql.Data.MySqlClient
Imports Mysqlx
Imports Mysqlx.Notice

Public Class Form4
    Private cumulativeGradePoints As Decimal = 0
    Private cumulativeCredits As Decimal = 0

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Form1.Show()

    End Sub
    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim query As String = "SELECT DISTINCT batch FROM students"
        Dim connection As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
        connection.Open()
        Dim command As New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()

        While reader.Read()
            ComboBox1.Items.Add(reader("batch"))
        End While
        reader.Close()
        connection.Close()

        query = "SELECT registration_number FROM students"
        connection.Open()
        command = New MySqlCommand(query, connection)
        reader = command.ExecuteReader()

        While reader.Read()
            ComboBox3.Items.Add(reader("registration_number"))
        End While
        reader.Close()
        connection.Close()
        ' Create a region for Button3
        Dim r As New Rectangle(0, 0, Button3.Width, Button3.Height)
        Dim gp As New GraphicsPath()
        gp.AddEllipse(r)
        Button3.Region = New Region(gp)
    End Sub




    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim semester As Integer = ComboBox2.SelectedItem
        Dim connectionString As String = "Server=localhost;Database=semester;User Id=root;Password=Tiger123;"
        Dim query As String = "SELECT id,name, credits FROM subjects WHERE semester = @semester"

        Using connection As New MySqlConnection(connectionString)
            connection.Open()
            Dim command As New MySqlCommand(query, connection)
            command.Parameters.AddWithValue("@semester", semester)

            Dim adapter As New MySqlDataAdapter(command)
            Dim dataTable As New DataTable()

            adapter.Fill(dataTable)

            DataGridView1.DataSource = dataTable
            DataGridView1.Columns(0).HeaderText = "Subject_id"
            DataGridView1.Columns(1).HeaderText = "Subject"
            DataGridView1.Columns(2).HeaderText = "Credits"

            ' Add a new column for entering grades
            Dim gradeColumn As New DataGridViewTextBoxColumn
            gradeColumn.HeaderText = "Grade"
            DataGridView1.Columns.Add(gradeColumn)
        End Using
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged_1(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        Dim selectedRegistrationNumber As String = ComboBox3.SelectedItem.ToString()
        Dim connectionString As String = "Server=localhost;Database=semester;User Id=root;Password=Tiger123;"
        Dim query As String = "SELECT name FROM students WHERE registration_number = @registrationNumber"

        Using connection As New MySqlConnection(connectionString)
            connection.Open()
            Dim command As New MySqlCommand(query, connection)
            command.Parameters.AddWithValue("@registrationNumber", selectedRegistrationNumber)

            Dim reader As MySqlDataReader = command.ExecuteReader()
            If reader.Read() Then
                TextBox1.Text = reader("name").ToString()
            End If
        End Using
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Initialize variables to store total credits and grade points
        Dim totalCredits As Decimal = 0
        Dim totalGradePoints As Decimal = 0

        ' Initialize cumulative variables to store previous semester's data
        Dim cumulativeCredits As Decimal = 0
        Dim cumulativeGradePoints As Decimal = 0

        ' Load previous semester's cumulative data from database
        Dim registrationNumber As String = ComboBox1.SelectedItem.ToString()
        Dim currentSemester As Integer = ComboBox2.SelectedItem.ToString() ' assuming current semester is 2
        Dim query As String = "SELECT SUM(s.credits) AS totalCredits, SUM(s.credits * " &
                     "CASE sr.grade " &
                     "WHEN 'O' THEN 10.0 " &
                     "WHEN 'A+' THEN 9.0 " &
                     "WHEN 'A' THEN 8.0 " &
                     "WHEN 'B+' THEN 7.0 " &
                     "WHEN 'B' THEN 6.0 " &
                     "WHEN 'C' THEN 5.0 " &
                     "ELSE 0.0 " &
                     "END) AS totalGradePoints " &
                     "FROM student_results sr " &
                     "JOIN subjects s ON sr.subject_id = s.id " &
                     "WHERE sr.registration_number = @registrationNumber AND s.semester < @semester"
        Dim parameters As New List(Of MySqlParameter)()

        parameters.Add(New MySqlParameter("@registrationNumber", registrationNumber))
        parameters.Add(New MySqlParameter("@semester", currentSemester))

        Using connection As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
            connection.Open()
            Using command As New MySqlCommand(query, connection)
                command.Parameters.AddRange(parameters.ToArray())
                Using reader As MySqlDataReader = command.ExecuteReader()
                    If reader.Read() Then
                        cumulativeCredits = If(reader("totalCredits") Is DBNull.Value, 0, Convert.ToDecimal(reader("totalCredits")))
                        cumulativeGradePoints = If(reader("totalGradePoints") Is DBNull.Value, 0, Convert.ToDecimal(reader("totalGradePoints")))
                    End If
                End Using
            End Using
        End Using

        ' Iterate through the DataGridView rows
        For Each row As DataGridViewRow In DataGridView1.Rows
            Dim creditsCell As DataGridViewCell = row.Cells(2)
            Dim gradeCell As DataGridViewCell = row.Cells(3)

            If creditsCell.Value IsNot Nothing AndAlso gradeCell.Value IsNot Nothing Then
                Dim credits As Decimal = Convert.ToDecimal(creditsCell.Value)
                Dim grade As String = gradeCell.Value.ToString()

                ' Calculate grade points based on the grade
                Dim gradePoints As Decimal
                Select Case grade
                    Case "O"
                        gradePoints = 10.0
                    Case "A+"
                        gradePoints = 9.0
                    Case "A"
                        gradePoints = 8.0
                    Case "B+"
                        gradePoints = 7.0
                    Case "B"
                        gradePoints = 6.0
                    Case "C"
                        gradePoints = 5.0
                    Case "F", "FA"
                        gradePoints = 0.00
                    Case Else
                        ' handle invalid grade
                End Select

                ' Update total credits and grade points
                totalCredits += credits
                totalGradePoints += credits * gradePoints
            End If
        Next

        ' Calculate SGPA
        Dim sgpa As Decimal = totalGradePoints / totalCredits

        ' Calculate cumulative credits and grade points
        cumulativeCredits += totalCredits
        cumulativeGradePoints += totalGradePoints

        ' Calculate CGPA
        Dim cgpa As Decimal = cumulativeGradePoints / cumulativeCredits

        ' Display SGPA and CGPA in textboxes
        TextBox2.Text = sgpa.ToString("F2")
        TextBox3.Text = cgpa.ToString("F2")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Using conn1 As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
            conn1.Open()

            ' Calculate the sgpa and cgpa values
            Dim sgpa As Double = CDbl(TextBox2.Text)
            Dim cgpa As Double = CDbl(TextBox3.Text)

            Dim errorOccurred As Boolean = False
            Dim errorMessage As String = ""

            ' Loop through the DataGridView rows
            For Each row As DataGridViewRow In DataGridView1.Rows
                ' Check if the data already exists in the table for the same semester
                Dim existsCmd As New MySqlCommand("SELECT COUNT(*) FROM student_results WHERE registration_number = @registration_number AND semester = @semester AND subject_id = @subject_id", conn1)
                existsCmd.Parameters.AddWithValue("@registration_number", ComboBox3.SelectedItem.ToString())
                existsCmd.Parameters.AddWithValue("@semester", ComboBox2.SelectedItem.ToString())
                Try
                    existsCmd.Parameters.AddWithValue("@subject_id", row.Cells(0).Value.ToString())
                Catch ex As Exception
                    ' Skip this iteration and continue with the next row
                    Continue For
                End Try

                Dim count As Integer = CInt(existsCmd.ExecuteScalar())
                If count > 0 Then
                    errorOccurred = True
                    errorMessage = "Error: Data already exists for the registration number '" & ComboBox3.SelectedItem.ToString() & "' and semester '" & ComboBox2.SelectedItem.ToString() & "'. Please try again."
                    Exit For
                Else
                    ' Create a MySqlCommand object with an INSERT statement
                    Dim cmd As New MySqlCommand("INSERT INTO student_results (registration_number, semester, subject_id, grade, sgpa, cgpa) VALUES (@registration_number, @semester, @subject_id, @grade, @sgpa, @cgpa)", conn1)

                    ' Add parameters for each column
                    cmd.Parameters.AddWithValue("@registration_number", ComboBox3.SelectedItem.ToString())
                    cmd.Parameters.AddWithValue("@semester", ComboBox2.SelectedItem.ToString())
                    Try
                        cmd.Parameters.AddWithValue("@subject_id", row.Cells(0).Value.ToString())
                    Catch ex As Exception
                        ' Skip this iteration and continue with the next row
                        Continue For
                    End Try
                    If row.Cells(3).Value IsNot Nothing Then
                        cmd.Parameters.AddWithValue("@grade", row.Cells(3).Value.ToString())
                    End If
                    cmd.Parameters.AddWithValue("@sgpa", sgpa)
                    cmd.Parameters.AddWithValue("@cgpa", cgpa)

                    ' Execute the INSERT command
                    cmd.ExecuteNonQuery()
                End If
            Next
            If Not errorOccurred Then
                MessageBox.Show("Data inserted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            If errorOccurred Then
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If

        End Using

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Using conn1 As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
            conn1.Open()

            ' Calculate the sgpa and cgpa values
            Dim sgpa As Double = CDbl(TextBox2.Text)
            Dim cgpa As Double = CDbl(TextBox3.Text)

            Dim errorOccurred As Boolean = False
            Dim errorMessage As String = ""

            ' Loop through the DataGridView rows
            For Each row As DataGridViewRow In DataGridView1.Rows
                ' Check if the data already exists in the table for the same semester
                Dim existsCmd As New MySqlCommand("SELECT COUNT(*) FROM student_results WHERE registration_number = @registration_number AND semester = @semester AND subject_id = @subject_id", conn1)
                existsCmd.Parameters.AddWithValue("@registration_number", ComboBox3.SelectedItem.ToString())
                existsCmd.Parameters.AddWithValue("@semester", ComboBox2.SelectedItem.ToString())
                Try
                    existsCmd.Parameters.AddWithValue("@subject_id", row.Cells(0).Value.ToString())
                Catch ex As Exception
                    ' Skip this iteration and continue with the next row
                    Continue For
                End Try

                Dim count As Integer = CInt(existsCmd.ExecuteScalar())
                If count > 0 Then
                    ' Update the existing record
                    Dim updateCmd As New MySqlCommand("UPDATE student_results SET grade = @grade, sgpa = @sgpa, cgpa = @cgpa WHERE registration_number = @registration_number AND semester = @semester AND subject_id = @subject_id", conn1)
                    updateCmd.Parameters.AddWithValue("@registration_number", ComboBox3.SelectedItem.ToString())
                    updateCmd.Parameters.AddWithValue("@semester", ComboBox2.SelectedItem.ToString())
                    updateCmd.Parameters.AddWithValue("@subject_id", row.Cells(0).Value.ToString())
                    updateCmd.Parameters.AddWithValue("@grade", row.Cells(3).Value.ToString())
                    updateCmd.Parameters.AddWithValue("@sgpa", sgpa)
                    updateCmd.Parameters.AddWithValue("@cgpa", cgpa)

                    ' Execute the UPDATE command
                    updateCmd.ExecuteNonQuery()
                Else
                    errorOccurred = True
                    errorMessage = "Error: No existing data found for the registration number '" & ComboBox3.SelectedItem.ToString() & "' and semester '" & ComboBox2.SelectedItem.ToString() & "'. Please try again."
                    Exit For
                End If
            Next
            If Not errorOccurred Then
                MessageBox.Show("Data updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            If errorOccurred Then
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Using
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Try
            ' Open the webpage in the default browser
            Process.Start("https://exam.pondiuni.edu.in/results/")
        Catch ex As Exception
            MessageBox.Show("Error opening website: " & ex.Message)
        End Try
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        DataGridView1.Columns.Clear()
        If DataGridView1.Rows.Count > 0 Then
            DataGridView1.Rows.Clear()
        Else

        End If
    End Sub
End Class