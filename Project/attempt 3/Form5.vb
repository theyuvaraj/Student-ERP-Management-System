Imports MySql.Data.MySqlClient
Imports Mysqlx
Imports System.Data.OleDb
Imports System.Text
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form5

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Form1.Show()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim batch As String = ComboBox5.SelectedItem
        Dim semester As Integer = ComboBox4.SelectedItem

        ' Generate the dynamic SQL query
        Dim query As String = "SELECT " &
                         "  s.registration_number, " &
                         "  s.name, "

        Dim subjectColumns As New List(Of String)

        ' Get the list of subjects for the selected semester
        Dim subjectQuery As String = "SELECT name FROM subjects WHERE semester = @semester"
        Dim parameters As New Dictionary(Of String, Object)
        parameters.Add("@semester", semester)
        Dim subjects As DataTable = ExecuteQuery(subjectQuery, parameters)

        For Each subject As DataRow In subjects.Rows
            subjectColumns.Add("MAX(CASE WHEN sub.name = '" & subject("name") & "' THEN sr.grade ELSE NULL END) AS '" & subject("name") & "'")
        Next

        query &= String.Join(", ", subjectColumns) & ", "
        query &= "sr.sgpa AS 'SGPA' "

        query &= "FROM " &
             "  students s " &
             "  LEFT JOIN student_results sr ON s.registration_number = sr.registration_number " &
             "  LEFT JOIN subjects sub ON sr.subject_id = sub.id " &
             "WHERE " &
                    "  sr.semester = @semester " &
                    "  AND s.batch = @batch " &
             "GROUP BY " &
             "  s.registration_number, s.name"

        parameters = New Dictionary(Of String, Object)
        parameters.Add("@batch", batch)
        parameters.Add("@semester", semester)

        Dim results As DataTable = ExecuteQuery(query, parameters)

        ' Bind the results to the DataGridView
        DataGridView1.DataSource = results
    End Sub
    Private Function ExecuteQuery(query As String, parameters As Dictionary(Of String, Object)) As DataTable
        Dim connectionString As String = "Server=localhost;Database=semester;User Id=root;Password=Tiger123;"
        Dim connection As New MySqlConnection(connectionString)
        Dim command As New MySqlCommand(query, connection)

        For Each parameter As KeyValuePair(Of String, Object) In parameters
            command.Parameters.AddWithValue(parameter.Key, parameter.Value)
        Next

        Dim results As New DataTable

        Try
            connection.Open()
            Dim adapter As New MySqlDataAdapter(command)
            adapter.Fill(results)
        Catch ex As Exception
            ' Handle the exception
        Finally
            connection.Close()
        End Try

        Return results
    End Function

    Private Sub Form5_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim query As String = "SELECT DISTINCT batch FROM students"
        Dim connection As New MySqlConnection("Server=localhost;Database=semester;User Id=root;Password=Tiger123;")
        connection.Open()
        Dim command As New MySqlCommand(query, connection)
        Dim reader As MySqlDataReader = command.ExecuteReader()

        While reader.Read()
            ComboBox5.Items.Add(reader("batch"))
        End While
        reader.Close()
        connection.Close()
    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        Dim batch As String = ComboBox5.SelectedItem
        Dim ranking As String = ComboBox2.SelectedItem ' assuming ComboBox6 is the ranking combobox

        ' Generate the dynamic SQL query
        Dim query As String = "SELECT " &
                         "  s.registration_number, " &
                         "  s.name, " &
                         "  MAX(sr.cgpa) AS 'CGPA' "

        query &= "FROM " &
             "  students s " &
             "  LEFT JOIN student_results sr ON s.registration_number = sr.registration_number " &
             "WHERE " &
             "  s.batch = @batch " &
             "GROUP BY " &
             "  s.registration_number, s.name " &
             "ORDER BY " &
             "  CGPA DESC "

        If ranking = "Top 10" Then
            query &= "LIMIT 10"
        ElseIf ranking = "Top 20" Then
            query &= "LIMIT 20"
        End If

        Dim parameters As New Dictionary(Of String, Object)
        parameters.Add("@batch", batch)

        Dim results As DataTable = ExecuteQuery(query, parameters)

        ' Bind the results to the DataGridView
        DataGridView1.DataSource = results
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        Form6.Show()
    End Sub
End Class